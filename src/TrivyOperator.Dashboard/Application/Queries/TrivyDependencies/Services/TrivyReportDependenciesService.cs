using TrivyOperator.Dashboard.Application.Queries.History.Shared;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Shared;
using TrivyOperator.Dashboard.Application.Queries.TrivyDependencies.Mappers;
using TrivyOperator.Dashboard.Application.Queries.TrivyDependencies.Models;
using TrivyOperator.Dashboard.Application.Queries.TrivyDependencies.Services.Abstractions;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Application.Queries.TrivyDependencies.Services;

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

        IImageReport?[] reports =
        [
            await GetImageReport(
                vulnerabilityReportProvider,
                digest,
                namespaceName,
                ctx),

            await GetImageReport(
                exposedSecretReportProvider,
                digest,
                namespaceName,
                ctx),

            await GetImageReport(
                sbomReportProvider,
                digest,
                namespaceName,
                ctx),
        ];

        ConfigAuditReport[] configAuditReports =
        [
            .. await TrivyQuerySupport.GetResources(
                configAuditReportProvider,
                namespaceName,
                ctx),
        ];

        List<IImageReport> imageReports =
        [
            .. reports.OfType<IImageReport>(),
        ];

        IReadOnlyList<SnapshotIndexEntry> snapshots = 
            await HistoryQuerySupport.GetResourceIndexes(vrHistoryStore, digest, namespaceName, ctx);

        DigestNode? digestNode =
            imageReports.ToDigestNode(
                digest,
                configAuditReports,
                snapshots);

        return digestNode is null
            ? null
            : new TrivyDependencyTreeDto
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
    
    private static async Task<TReport?> GetImageReport<TReport>(
        IResourceProvider<TReport, Digest> provider, 
        Digest digest,
        string? namespaceName,
        CancellationToken ctx = default)
        where TReport : class, IImageReport
    {
        TReport? report = await provider.GetResourceSummary(digest, ctx);
        NamespaceName namespaceValue = new(namespaceName);
        return namespaceName is null || report?.HasNamespaceName(namespaceValue) == true
            ? report
            : null;
    }
}
