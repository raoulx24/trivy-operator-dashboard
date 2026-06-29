using System.Collections.Concurrent;
using TrivyOperator.Dashboard.Application.Trivy.Models;
using TrivyOperator.Dashboard.Application.Trivy.Services.ConfigAuditReport.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy;
using TrivyOperator.Dashboard.Domain.TrivyOld;
using TrivyOperator.Dashboard.Domain.TrivyOld.ConfigAuditReport;
using TrivyOperator.Dashboard.Infrastructure.Caching.Abstractions;

namespace TrivyOperator.Dashboard.Application.Trivy.Services.ConfigAuditReport;

public class ConfigAuditReportService(IConcurrentDictionaryCache<ConfigAuditReportCr> cache) : IConfigAuditReportService
{
    public Task<IEnumerable<ConfigAuditReportDto>> GetConfigAuditReportDtos(
        string? namespaceName = null,
        IEnumerable<int>? excludedSeverities = null
    )
    {
        excludedSeverities ??= [];
        int[] excludedSeveritiesArray = [.. excludedSeverities,];
        bool hasExcludedSeverities = excludedSeveritiesArray.Length != 0;
        int[] includedSeverities = [.. Enum.GetValues<TrivySeverity>().Cast<int>().Except(excludedSeveritiesArray),];

        IEnumerable<ConfigAuditReportCr> cachedValues =
        [
            .. cache.Where(kvp => string.IsNullOrEmpty(namespaceName) || kvp.Key == namespaceName)
                .SelectMany(kvp => kvp.Value.Values),
        ];

        IEnumerable<ConfigAuditReportDto> values = cachedValues.Select(cr => cr.ToConfigAuditReportDto())
            .Select(dto =>
                {
                    dto.Details =
                    [
                        .. dto.Details.Join(
                            includedSeverities,
                            vulnerability => vulnerability.SeverityId,
                            id => id,
                            (vulnerability, _) => vulnerability
                        ),
                    ];
                    return dto;
                }
            )
            .Where(dto => !hasExcludedSeverities || dto.Details.Length != 0);

        return Task.FromResult(values);
    }

    public Task<ConfigAuditReportDto?> GetConfigAuditReportDtoByUid(Guid uid)
    {
        string[] namespaceNames = [.. cache.Where(x => !x.Value.IsEmpty).Select(x => x.Key),];

        foreach (string namespaceName in namespaceNames)
        {
            if (cache.TryGetValue(
                    namespaceName,
                    out ConcurrentDictionary<string, ConfigAuditReportCr>? configAuditReportCrs
                ))
            {
                if (configAuditReportCrs.TryGetValue(uid.ToString(), out ConfigAuditReportCr? configAuditReportCr))
                {
                    return Task.FromResult<ConfigAuditReportDto?>(configAuditReportCr.ToConfigAuditReportDto());
                }
            }
        }

        return Task.FromResult<ConfigAuditReportDto?>(null);
    }


    public Task<IEnumerable<ConfigAuditReportDenormalizedDto>> GetConfigAuditReportDenormalizedDtos(
        string? namespaceName = null
    )
    {
        IEnumerable<ConfigAuditReportCr> cachedValues =
        [
            .. cache.Where(kvp => string.IsNullOrEmpty(namespaceName) || kvp.Key == namespaceName)
                .SelectMany(kvp => kvp.Value.Values),
        ];
        IEnumerable<ConfigAuditReportDenormalizedDto> values =
            cachedValues.SelectMany(car => car.ToConfigAuditReportDetailDenormalizedDtos());

        return Task.FromResult(values);
    }

    public Task<IEnumerable<string>> GetActiveNamespaces() =>
        Task.FromResult<IEnumerable<string>>([.. cache.Where(x => !x.Value.IsEmpty).Select(x => x.Key),]);

    public Task<IEnumerable<ConfigAuditReportSummaryDto>> GetConfigAuditReportSummaryDtos()
    {
        ConfigAuditReportCr[] cachedValues = [.. cache.SelectMany(kvp => kvp.Value.Values),];

        var valuesByNs = cachedValues.Select(car => car.ToConfigAuditReportDto())
            .SelectMany(dto => dto.Details.Select(detail => new
                    {
                        NamespaceName = dto.ResourceNamespace,
                        Kind = dto.ResourceKind,
                        detail.SeverityId,
                        detail.CheckId,
                    }
                )
            )
            .GroupBy(key => new
                {
                    key.NamespaceName,
                    key.Kind,
                    key.SeverityId,
                }
            )
            .Select(group => new
                {
                    ns = group.Key.NamespaceName,
                    kind = group.Key.Kind,
                    severityId = group.Key.SeverityId,
                    totalCount = group.Count(),
                    distinctCount = group.Select(x => x.CheckId).Distinct().Count(),
                }
            )
            .ToArray();

        string[] allNamespaces = [.. valuesByNs.Select(x => x.ns).Distinct(),];
        string[] allKinds = [.. valuesByNs.Select(x => x.kind).Distinct(),];
        int[] allSeverities = [.. Enum.GetValues<TrivySeverity>().Cast<int>().Where(x => x < 4),];


        var allCombinationsWithNs = allNamespaces.SelectMany(
                _ => allKinds,
                (ns, kind) => new
                {
                    ns,
                    kind,
                }
            )
            .SelectMany(
                _ => allSeverities,
                (nk, severityId) => new
                {
                    nk.ns,
                    nk.kind,
                    severityId,
                }
            );

        IEnumerable<ConfigAuditReportSummaryDto> resultByNs = allCombinationsWithNs.GroupJoin(
            valuesByNs,
            combo => new
            {
                combo.ns,
                combo.kind,
                combo.severityId,
            },
            count => new
            {
                count.ns,
                count.kind,
                count.severityId,
            },
            (combo, countGroup) =>
            {
                var countGroupArray = countGroup.ToArray();
                return new ConfigAuditReportSummaryDto
                {
                    NamespaceName = combo.ns,
                    Kind = combo.kind,
                    SeverityId = combo.severityId,
                    TotalCount = countGroupArray.FirstOrDefault()?.totalCount ?? 0,
                    DistinctCount = countGroupArray.FirstOrDefault()?.distinctCount ?? 0,
                };
            }
        );

        var allCombinationsForTotals = allKinds.SelectMany(
            _ => allSeverities,
            (kind, severityId) => new
            {
                kind,
                severityId,
            }
        );

        var valueTotals = cachedValues.Select(car => car.ToConfigAuditReportDto())
            .SelectMany(dto => dto.Details.Select(detail => new
                    {
                        Kind = dto.ResourceKind,
                        detail.SeverityId,
                        detail.CheckId,
                    }
                )
            )
            .GroupBy(key => new
                {
                    key.Kind,
                    key.SeverityId,
                }
            )
            .Select(group => new
                {
                    kind = group.Key.Kind,
                    severityId = group.Key.SeverityId,
                    totalCount = group.Count(),
                    distinctCount = group.Select(x => x.CheckId).Distinct().Count(),
                }
            );

        IEnumerable<ConfigAuditReportSummaryDto> resultsTotal = allCombinationsForTotals.GroupJoin(
            valueTotals,
            combo => new
            {
                combo.kind,
                combo.severityId,
            },
            count => new
            {
                count.kind,
                count.severityId,
            },
            (combo, countGroup) =>
            {
                var countGroupArray = countGroup.ToArray();
                return new ConfigAuditReportSummaryDto
                {
                    NamespaceName = string.Empty,
                    Kind = combo.kind,
                    SeverityId = combo.severityId,
                    TotalCount = countGroupArray.FirstOrDefault()?.totalCount ?? 0,
                    DistinctCount = countGroupArray.FirstOrDefault()?.distinctCount ?? 0,
                };
            }
        );

        return Task.FromResult(resultByNs.Concat(resultsTotal));
    }
}
