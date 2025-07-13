﻿using TrivyOperator.Dashboard.Application.Models;
using TrivyOperator.Dashboard.Application.Services.Trivy.ExposedSecretReport.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy;
using TrivyOperator.Dashboard.Domain.Trivy.ExposedSecretReport;
using TrivyOperator.Dashboard.Infrastructure.Abstractions;

namespace TrivyOperator.Dashboard.Application.Services.Trivy.ExposedSecretReport;

public class ExposedSecretReportService(IConcurrentDictionaryCache<ExposedSecretReportCr> cache)
    : IExposedSecretReportService
{
    public Task<IEnumerable<ExposedSecretReportDto>> GetExposedSecretReportDtos(
        string? namespaceName = null,
        IEnumerable<int>? excludedSeverities = null)
    {
        excludedSeverities ??= [];
        int[] excludedSeveritiesArray = [.. excludedSeverities,];
        int[] includedSeverities = [.. Enum.GetValues<TrivySeverity>().Cast<int>().Except(excludedSeveritiesArray),];

        IEnumerable<ExposedSecretReportCr> cachedValues = [.. cache
            .Where(kvp => string.IsNullOrEmpty(namespaceName) || kvp.Key == namespaceName)
            .SelectMany(kvp => kvp.Value.Values),];
        IEnumerable<ExposedSecretReportDto> dtos = cachedValues
            .Select(cr => cr.ToExposedSecretReportDto())
                    .Select(
                        dto =>
                        {
                            dto.Details = [.. dto.Details.Join(
                                    includedSeverities,
                                    vulnerability => vulnerability.SeverityId,
                                    id => id,
                                    (vulnerability, _) => vulnerability),];
                            return dto;
                        })
                    .Where(dto => excludedSeveritiesArray.Length == 0 || dto.Details.Length != 0);

        return Task.FromResult(dtos);
    }

    public Task<IEnumerable<ExposedSecretReportDenormalizedDto>> GetExposedSecretDenormalizedDtos(
        string? namespaceName = null)
    {
        IEnumerable<ExposedSecretReportCr> cachedValues = [.. cache
            .Where(kvp => string.IsNullOrEmpty(namespaceName) || kvp.Key == namespaceName)
            .SelectMany(kvp => kvp.Value.Values),];
        IEnumerable<ExposedSecretReportDenormalizedDto> result = cachedValues
            .SelectMany(cr => cr.ToExposedSecretReportDenormalizedDtos());

        return Task.FromResult(result);
    }

    public Task<IEnumerable<string>> GetActiveNamespaces() =>
        Task.FromResult<IEnumerable<string>>([.. cache.Where(x => !x.Value.IsEmpty).Select(x => x.Key),]);

    public Task<IEnumerable<ExposedSecretReportImageDto>> GetExposedSecretReportImageDtos(
        string? namespaceName = null,
        IEnumerable<int>? excludedSeverities = null)
    {
        excludedSeverities ??= [];
        int[] excludedSeveritiesArray = [.. excludedSeverities,];
        int[] incudedSeverities = [.. Enum.GetValues<TrivySeverity>().Cast<int>().Except(excludedSeveritiesArray),];

        IEnumerable<ExposedSecretReportCr> cachedValues = [.. cache
            .Where(kvp => string.IsNullOrEmpty(namespaceName) || kvp.Key == namespaceName)
            .SelectMany(kvp => kvp.Value.Values),];

        IEnumerable<ExposedSecretReportImageDto> exposedSecretReportImageDtos = cachedValues
            .GroupBy(esr => esr.Report?.Artifact?.Digest)
                    .Select(group => group.ToExposedSecretReportImageDto())
                    .Select(
                        esrDto =>
                        {
                            esrDto.Details = [.. esrDto.Details.Join(
                                    incudedSeverities,
                                    vulnerability => vulnerability.SeverityId,
                                    id => id,
                                    (vulnerability, _) => vulnerability),];
                            return esrDto;
                        })
                    .Where(esrDto => excludedSeveritiesArray.Length == 0 || esrDto.Details.Length != 0);

        return Task.FromResult(exposedSecretReportImageDtos);
    }

    public Task<IEnumerable<EsSeveritiesByNsSummaryDto>> GetExposedSecretReportSummaryDtos()
    {
        int[] severityIds = [.. Enum.GetValues<TrivySeverity>().Cast<int>().Where(x => x < 4),];

        ExposedSecretReportCr[] cachedValues = [.. cache.SelectMany(kvp => kvp.Value.Values),];

        IEnumerable<EsSeveritiesByNsSummaryDto> summaryDtos = cachedValues.SelectMany(
                    es => (es.Report?.Secrets ?? []).Select(
                        esd => new { es.Metadata.NamespaceProperty, esd.Severity, esd.RuleId, }))
            .GroupBy(item => new { item.NamespaceProperty, item.Severity, })
            .Select(
                group => new
                {
                    namespaceName = group.Key.NamespaceProperty,
                    trivySeverityId = group.Key.Severity,
                    totalCount = group.Count(),
                    distinctCount = group.Select(item => item.RuleId).Distinct().Count(),
                })
            .GroupBy(x => x.namespaceName)
            .SelectMany(
                g => severityIds.Select(
                    SeverityId => new
                    {
                        NamespaceName = g.Key,
                        SeverityId,
                        TotalCount = g.FirstOrDefault(x => (int)x.trivySeverityId == SeverityId)?.totalCount ?? 0,
                        DistinctCount = g.FirstOrDefault(x => (int)x.trivySeverityId == SeverityId)?.distinctCount ?? 0,
                    }))
            .GroupBy(last => last.NamespaceName)
            .Select(
                summaryGroup =>
                {
                    EsSeveritiesByNsSummaryDto essns = new()
                    {
                        Uid = Guid.NewGuid(),
                        NamespaceName = summaryGroup.Key,
                        Details = summaryGroup.Select(
                                detail =>
                                {
                                    EsSeveritiesByNsSummaryDetailDto detailDto = new()
                                    {
                                        Id = detail.SeverityId,
                                        TotalCount = detail.TotalCount,
                                        DistinctCount = detail.DistinctCount,
                                    };
                                    return detailDto;
                                }),
                        IsTotal = false,
                    };
                    return essns;
                });
        EsSeveritiesByNsSummaryDetailDto[] totalSummary = cachedValues.SelectMany(
                    es => (es.Report?.Secrets ?? []).Select(esd => new { esd.Severity, esd.RuleId, }))
            .GroupBy(item => item.Severity)
            .Select(
                group => new EsSeveritiesByNsSummaryDetailDto
                {
                    Id = (int)group.Key,
                    TotalCount = group.Count(),
                    DistinctCount = group.Select(item => item.RuleId).Distinct().Count(),
                })
            .ToArray();

        var detailDtos = totalSummary
            .Concat(severityIds
                .Where(id => totalSummary.All(x => x.Id != id))
                .Select(id => new EsSeveritiesByNsSummaryDetailDto { Id = id, TotalCount = 0, DistinctCount = 0, }));
        var summaryDto = new EsSeveritiesByNsSummaryDto
        {
            Uid = Guid.NewGuid(),
            NamespaceName = string.Empty,
            Details = detailDtos,
            IsTotal = true,
        };
        summaryDtos = summaryDtos.Concat([summaryDto,]);

        return Task.FromResult(summaryDtos);
    }
}
