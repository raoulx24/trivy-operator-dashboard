using TrivyOperator.Dashboard.Application.Trivy.Models;
using TrivyOperator.Dashboard.Application.Trivy.Services.ClusterSbomReport.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy;
using TrivyOperator.Dashboard.Domain.TrivyOld;
using TrivyOperator.Dashboard.Domain.TrivyOld.ClusterSbomReport;
using TrivyOperator.Dashboard.Domain.TrivyOld.ClusterVulnerabilityReport;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.Abstractions;

namespace TrivyOperator.Dashboard.Application.Trivy.Services.ClusterSbomReport;

public class ClusterSbomReportService(
    IConcurrentDictionaryCache<ClusterSbomReportCr> cache,
    IConcurrentDictionaryCache<ClusterVulnerabilityReportCr> cvrCache
) : IClusterSbomReportService
{
    public Task<IEnumerable<ClusterSbomReportDto>> GetClusterSbomReportDtos()
    {
        IEnumerable<ClusterSbomReportCr> cachedValues = [.. cache.SelectMany(kvp => kvp.Value.Values),];
        IEnumerable<ClusterVulnerabilityReportCr> chachedCvrValues = [.. cvrCache.SelectMany(kvp => kvp.Value.Values),];

        IEnumerable<ClusterSbomReportDto> values = cachedValues.Select(cr => cr.ToClusterSbomReportDto())
            .GroupJoin(
                chachedCvrValues,
                sbom => sbom.Uid.ToString(),
                cvr => cvr.Metadata.OwnerReferences?.FirstOrDefault()?.Uid ?? "unknown",
                (sbom, cvrGroup) =>
                {
                    ClusterSbomReportDto sbomDto = sbom;
                    ClusterVulnerabilityReportCr? cvr = cvrGroup.FirstOrDefault();
                    if (cvr != null)
                    {
                        sbomDto.HasVulnerabilities = true;
                        sbomDto.CriticalCount = cvr.Report?.Summary?.CriticalCount ?? -1;
                        sbomDto.HighCount = cvr.Report?.Summary?.HighCount ?? -1;
                        sbomDto.MediumCount = cvr.Report?.Summary?.MediumCount ?? -1;
                        sbomDto.LowCount = cvr.Report?.Summary?.LowCount ?? -1;
                        sbomDto.UnknownCount = cvr.Report?.Summary?.UnknownCount ?? -1;

                        foreach (Vulnerability cvrVulnerability in cvr.Report?.Vulnerabilities ?? [])
                        {
                            IEnumerable<ClusterSbomReportDetailDto> sbomDtoBoomRefs = sbomDto.Details.Where(detail =>
                                detail.Name == cvrVulnerability.Resource &&
                                detail.Version == cvrVulnerability.InstalledVersion
                            );
                            foreach (ClusterSbomReportDetailDto sbomDtoBoomRef in sbomDtoBoomRefs)
                            {
                                switch (cvrVulnerability.Severity)
                                {
                                    case TrivySeverity.CRITICAL:
                                        sbomDtoBoomRef.CriticalCount = Math.Max(1, sbomDtoBoomRef.CriticalCount + 1);
                                        break;
                                    case TrivySeverity.HIGH:
                                        sbomDtoBoomRef.HighCount = Math.Max(1, sbomDtoBoomRef.HighCount + 1);
                                        break;
                                    case TrivySeverity.MEDIUM:
                                        sbomDtoBoomRef.MediumCount = Math.Max(1, sbomDtoBoomRef.MediumCount + 1);
                                        break;
                                    case TrivySeverity.LOW:
                                        sbomDtoBoomRef.LowCount = Math.Max(1, sbomDtoBoomRef.LowCount + 1);
                                        break;
                                    case TrivySeverity.UNKNOWN:
                                        sbomDtoBoomRef.UnknownCount = Math.Max(1, sbomDtoBoomRef.UnknownCount + 1);
                                        break;
                                }
                            }
                        }
                    }

                    return sbomDto;
                }
            );

        return Task.FromResult(values);
    }

    public Task<IEnumerable<ClusterSbomReportDenormalizedDto>> GetClusterSbomReportDenormalizedDtos()
    {
        IEnumerable<ClusterSbomReportCr> cachedValues = [.. cache.SelectMany(kvp => kvp.Value.Values),];
        IEnumerable<ClusterSbomReportDenormalizedDto> values =
            cachedValues.SelectMany(cr => cr.ToClusterSbomReportDenormalizedDtos());
        return Task.FromResult(values);
    }
}
