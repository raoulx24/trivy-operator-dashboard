using TrivyOperator.Dashboard.Application.Queries.Trivy.Mappers;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ClusterSbomReports.Abstractions;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ClusterSbomReports;

public class ClusterSbomReportService(
    IResourceProvider<ClusterSbomReport, Uid> resourceProvider,
    IResourceProvider<ClusterVulnerabilityReport, Uid> cvrResourceProvider
) : IClusterSbomReportService
{
    public async Task<IEnumerable<SbomReportImageMinimalDto>> GetClusterSbomReportMinimalDtos(CancellationToken ctx = default)
    {
        IReadOnlyList<ClusterSbomReport> resourceSummaries = await resourceProvider.GetResourceSummaries(ctx);
        IReadOnlyList<ClusterVulnerabilityReport> cvrResourceSummaries =
            await cvrResourceProvider.GetResourceSummaries(ctx);

        Dictionary<Uid, SeverityCounters> severityCountersByUid =
            cvrResourceSummaries.ToDictionary(
                x => x.Id,
                x => x.SeverityCounters);

        return resourceSummaries
            .Select(x =>
            {
                SeverityCounters? severityCounters = x.Occurrence.Metadata.OwnerReferences?
                    .Select(owner =>
                        severityCountersByUid.TryGetValue(owner.Uid, out SeverityCounters counters)
                            ? (SeverityCounters?)counters
                            : null)
                    .FirstOrDefault();

                return x.ToMinimalDto(severityCounters);
            });
    }
    
    public async Task<IEnumerable<ClusterSbomReportDto>> GetClusterSbomReportDtos(
        CancellationToken ctx = default)
    {
        IReadOnlyList<ClusterSbomReport> reports =
            await resourceProvider.GetResources(ctx);

        HashSet<Uid> vulnerabilityReportIds =
            [.. await cvrResourceProvider.GetResourceIds(ctx),];

        List<ClusterSbomReportDto> result = [];

        foreach (ClusterSbomReport report in reports)
        {
            OwnerReference? ownerReference =
                report.Occurrence.Metadata.OwnerReferences?
                    .FirstOrDefault(owner => vulnerabilityReportIds.Contains(owner.Uid));

            ClusterVulnerabilityReport? vulnerabilityReport =
                ownerReference != null
                    ? await cvrResourceProvider.GetResource(
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
            
            result.Add(report.ToDto(vulnerabilityReport?.SeverityCounters, severities));
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
