using TrivyOperator.Dashboard.Application.Queries.Trivy.Mappers;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.Shared;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.TrivyReportDependencies.Abstractions;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Services.TrivyReportDependencies;

public sealed class TrivyReportDependenciesService(
    IResourceProvider<VulnerabilityReport, Digest> vulnerabilityReportProvider,
    IResourceProvider<ExposedSecretReport, Digest> exposedSecretReportProvider,
    IResourceProvider<SbomReport, Digest> sbomReportProvider,
    IResourceProvider<ConfigAuditReport, Uid> configAuditReportProvider,
    IVulnerabilityReportsHistoryStore vrHistoryStore
) : ITrivyReportDependenciesService
{
    public async Task<TrivyDependencyTreeDto?> GetTrivyDependencyTree(
        string imageDigest,
        string? namespaceName = null,
        CancellationToken ctx = default)
    {
        Digest digest = new(imageDigest);
        NamespaceName namespaceValue = new(namespaceName);

        // Digest-keyed reports are already cluster-wide aggregates.
        IImageReport?[] reports =
        [
            await GetImageReport(vulnerabilityReportProvider, digest, namespaceName, ctx),
            await GetImageReport(exposedSecretReportProvider, digest, namespaceName, ctx),
            await GetImageReport(sbomReportProvider, digest, namespaceName, ctx),
        ];

        ConfigAuditReport[] configAuditReports =
        [
            .. await TrivyQuerySupport.GetResources(configAuditReportProvider, namespaceName, ctx),
        ];

        List<IImageReport> imageReports =
        [
            .. reports.OfType<IImageReport>(),
        ];
            
        DigestNode? digestNode =
            imageReports.ToDigestNode(digest);

        if (digestNode is null)
            return null;

        digestNode.TrivyReports = imageReports.ToTrivyReportNodes();

        digestNode.Workloads =
            imageReports.ToWorkloadsNode(configAuditReports);

        IReadOnlyList<SnapshotIndexEntry> snapshots =
            await vrHistoryStore.GetSnapshotIndexesAsync(digest, ctx);

        IReadOnlyList<SnapshotIndexEntry> filteredSnapshots =
        [
            .. snapshots.Where(x =>
                x.HistoryMetadata.NamespaceNames.Contains(namespaceValue)),
        ];

        digestNode.VrHistory =
            filteredSnapshots.ToVrHistoryNode();

        return new TrivyDependencyTreeDto
        {
            Digest = digestNode,
        };
    }

    public async Task<bool> TrivyDependenciesExist(
        string imageDigest,
        string? namespaceName = null,
        CancellationToken ct = default)
    {
        Digest digest = new(imageDigest);

        // These reports are digest aggregates, so namespace filtering is not
        // applicable at the provider level.
        if (await vulnerabilityReportProvider.GetResource(digest, ct) is not null)
            return true;

        if (await exposedSecretReportProvider.GetResource(digest, ct) is not null)
            return true;

        return await sbomReportProvider.GetResource(digest, ct) is not null;
    }
    
    private async Task<TReport?> GetImageReport<TReport>(
        IResourceProvider<TReport, Digest> provider, 
        Digest digest,
        string? namespaceName,
        CancellationToken ctx = default)
        where TReport : class, IImageReport
    {
        var report = await provider.GetResourceSummary(digest, ctx);
        NamespaceName namespaceValue = new NamespaceName(namespaceName);
        return namespaceName is null || report?.HasNamespaceName(namespaceValue) == true
            ? report
            : null;
    }
}
