using k8s;
using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.Kubernetes.Contexts.Abstractions;
using TrivyOperator.Dashboard.Application.ScanOnDemand.Abstractions;
using TrivyOperator.Dashboard.Application.ScanOnDemand.Models;
using TrivyOperator.Dashboard.Application.ScanOnDemand.Options;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.ScanOnDemand.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.ClientFactory.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.CustomResources;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Mappers.Abstract;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Services.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.ToDomain;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.ToDomain.Extensions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ExposedSecretReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.SbomReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.VulnerabilityReports.Models;

namespace TrivyOperator.Dashboard.Infrastructure.ScanOnDemand.Kubernetes;

// reads the reports of the target container of a scan pod, in the scan namespace, by label selector
// the mappers are created here, not resolved: DI registers them only for the report types enabled in the main pipeline
// always in the default context
public sealed class ScanReportReader(
    IKubernetesClientFactory kubernetesClientFactory,
    IContextProvider contextProvider,
    ICrdFactory crdFactory,
    IOptions<ScanOnDemandOptions> options,
    ILogger<ScanReportReader> logger
) : IScanReportReader
{
    private static readonly VulnerabilityReportMapper VulnerabilityReportMapper = new();
    private static readonly ExposedSecretReportMapper ExposedSecretReportMapper = new();
    private static readonly SbomReportMapper SbomReportMapper = new();

    // throws when the reports found point to different digests
    public async Task<ScanReports> ReadAsync(
        ResourceName podName,
        IReadOnlySet<ScanReportType> types,
        CancellationToken ctx = default)
    {
        VulnerabilityReport? vulnerabilityReport = types.Contains(ScanReportType.VulnerabilityReport)
            ? await ReadOneAsync<VulnerabilityReportCr, VulnerabilityReport>(VulnerabilityReportMapper, podName, ctx)
            : null;

        ExposedSecretReport? exposedSecretReport = types.Contains(ScanReportType.ExposedSecretReport)
            ? await ReadOneAsync<ExposedSecretReportCr, ExposedSecretReport>(ExposedSecretReportMapper, podName, ctx)
            : null;

        SbomReport? sbomReport = types.Contains(ScanReportType.SbomReport)
            ? await ReadOneAsync<SbomReportCr, SbomReport>(SbomReportMapper, podName, ctx)
            : null;

        Digest[] digests =
        [
            .. new[] { vulnerabilityReport?.ImageDigest, exposedSecretReport?.ImageDigest, sbomReport?.ImageDigest, }
                .OfType<Digest>()
                .Distinct(),
        ];

        if (digests.Length > 1)
        {
            throw new InvalidOperationException(
                $"The reports of scan pod {podName} point to different digests: {string.Join(", ", digests)}.");
        }

        return new ScanReports(
            digests.Length == 1 ? digests[0] : new Digest(null),
            vulnerabilityReport,
            exposedSecretReport,
            sbomReport);
    }

    // the report is owned by the scan pod (owner reference) and is the target container's (the container name, read the
    // same way as in the main pipeline: ToContainerName); the init container's reports have the same owner
    // one per type is expected; more than one: the newest, with a warning
    // only the matching report is mapped; the list itself holds every report of the type in the (dedicated) namespace
    private async Task<TReport?> ReadOneAsync<TCr, TReport>(
        IResourceMapper<TCr, TReport> mapper,
        ResourceName podName,
        CancellationToken ctx)
        where TCr : CustomResource
        where TReport : class, IEntity
    {
        CustomResourceDefinition crd = crdFactory.Get<TCr>();

        CustomResourceList<TCr> list = await GetClient()
            .ListNamespacedCustomObjectAsync<CustomResourceList<TCr>>(
                crd.Group,
                crd.Version,
                options.Value.NamespaceName,
                crd.PluralName,
                cancellationToken: ctx);

        ContainerName targetContainer = new(ScanPodLabels.TargetContainerName);

        List<TCr> items =
        [
            .. (list.Items ?? []).Where(x =>
                x.Metadata is not null
                && x.Metadata.ToReportMetadata().OwnerReferences.Any(o => o.Kind.Value == "Pod" && o.Name == podName)
                && x.Metadata.ToContainerName() == targetContainer),
        ];

        if (items.Count == 0)
        {
            return null;
        }

        if (items.Count > 1)
        {
            logger.LogWarning(
                "Scan pod {podName} has {count} {kind} reports; the newest is used",
                podName,
                items.Count,
                crd.PluralName);
        }

        TCr newest = items.MaxBy(static x => x.Metadata?.CreationTimestamp ?? DateTime.MinValue)!;

        return mapper.MapToDomain(newest, null);
    }

    private k8s.Kubernetes GetClient() => kubernetesClientFactory.GetClient(contextProvider.GetDefaultContext());
}
