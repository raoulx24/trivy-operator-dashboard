using TrivyOperator.Dashboard.Application.Queries.Trivy.Mappers;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ClusterSbomReports.Abstractions;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ClusterSbomReports;

public class ClusterSbomReportService(
    IResourceProvider<ClusterSbomReport, Uid> resourceProvider,
    IResourceProvider<ClusterVulnerabilityReport, Uid> vulnerabilityResourceProvider
) : IClusterSbomReportService
{
    public async Task<IEnumerable<SbomReportImageMinimalDto>> GetSbomReportImageMinimalDtos(CancellationToken ctx = default)
    {
        IReadOnlyList<ClusterSbomReport> resourceSummaries = await resourceProvider.GetResourceSummaries(ctx);
        HashSet<Uid> vrDigests = [.. await vulnerabilityResourceProvider.GetResourceIds(ctx),];

        return resourceSummaries
            .Select(x => x.ToMinimalDto(
                x.Occurrence.Resource.OwnerReferences?
                    .Any(owner => vrDigests.Contains(owner.Uid)) == true));
    }
    
    public async Task<IEnumerable<ClusterSbomReportDto>> GetClusterSbomReportDtos(
        CancellationToken ctx = default)
    {
        IReadOnlyList<ClusterSbomReport> reports =
            await resourceProvider.GetResourceSummaries(ctx);

        HashSet<Uid> vulnerabilityReportIds =
            [.. await vulnerabilityResourceProvider.GetResourceIds(ctx)];

        List<ClusterSbomReportDto> result = [];

        foreach (ClusterSbomReport report in reports)
        {
            OwnerReference? ownerReference =
                report.Occurrence.Resource.OwnerReferences?
                    .FirstOrDefault(owner => vulnerabilityReportIds.Contains(owner.Uid));

            ClusterVulnerabilityReport? vulnerabilityReport =
                ownerReference != null
                    ? await vulnerabilityResourceProvider.GetResource(
                        ownerReference.Value.Uid,
                        ctx)
                    : null;

            Dictionary<Purl, SeverityCounters> severities =
                vulnerabilityReport?.Vulnerabilities
                    .GroupBy(v => v.ScannedPackage.Purl)
                    .ToDictionary(
                        g => g.Key,
                        g => new SeverityCounters(g.Select(v => v.Severity))
                    )
                ?? [];

            result.Add(report.ToDto(severities));
        }

        return result;
    }

    public async Task<IEnumerable<ClusterSbomReportDenormalizedDto>> GetClusterSbomReportDenormalizedDtos(
        CancellationToken ctx = default)
    {
        IReadOnlyList<ClusterSbomReport> reports =
            await resourceProvider.GetResourceSummaries(ctx);

        return reports.SelectMany(report => report.ToDenormalizedDtos());
    }
}
