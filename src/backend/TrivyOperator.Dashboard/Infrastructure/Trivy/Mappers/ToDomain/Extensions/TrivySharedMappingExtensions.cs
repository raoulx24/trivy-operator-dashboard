using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.Abstracts;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Shared;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.ToDomain.Extensions;

public static class TrivySharedMappingExtensions
{
    public static Uid ToUidKey(this IKubernetesObject<V1ObjectMeta> cr)
    {
        return new Uid(cr.Metadata.Uid);
    }
    
    public static Digest ToDigestKey(this IHasArtifact cr)
    {
        return new Digest(cr.Artifact.Digest);
    }

    public static ReportMetadata ToReportMetadata(this V1ObjectMeta metadata)
    {
        return new ReportMetadata(
            new ResourceName(metadata.Name),
            new NamespaceName(metadata.NamespaceProperty),
            new Timestamp(metadata.CreationTimestamp ?? DateTime.MinValue),
            new Uid(metadata.Uid),
            metadata.ToOwnerReferences());
    }
    
    public static ContainerName ToContainerName(this V1ObjectMeta metadata)
    {
        IDictionary<string, string>? labels = metadata.Labels;

        return new ContainerName(Get("trivy-operator.container.name"));

        string Get(string key)
            => labels != null && labels.TryGetValue(key, out string? v)
                ? v
                : string.Empty;
    }

    public static ImageMeta ToImageMeta(this ArtifactCr artifact, RegistryCr? registry)
    {
        return new ImageMeta(
            new ImageRegistry(registry?.Server),
            new ImageRepository(artifact.Repository),
            new ImageTag(artifact.Tag));
    }
    
    public static Digest ToDigest(this ArtifactCr artifact)
    {
        return new Digest(artifact.Digest);
    }

    public static Scanner ToScanner(this ScannerCr? scanner)
    {
        return new Scanner(
            new ScannerName(scanner?.Name),
            new ScannerVendor(scanner?.Vendor),
            new ScannerVersion(scanner?.Version));
    }
    
    internal static SeverityCounters ToSeverityCounters(this SummaryCr? cr)
    {
        return new SeverityCounters(
            criticalCount: cr?.CriticalCount ?? 0,
            highCount: cr?.HighCount ?? 0,
            mediumCount: cr?.MediumCount ?? 0,
            lowCount: cr?.LowCount ?? 0,
            unknownCount: cr?.UnknownCount ?? 0,
            noneCount: cr?.NoneCount ?? 0
        );
    }
    
    public static Timestamp ResolveTimestamp(params string?[] values)
    {
        foreach (string? value in values)
        {
            if (DateTime.TryParse(value, out var timestamp))
                return new Timestamp(timestamp);
        }

        throw new InvalidOperationException("None of the provided timestamps could be parsed.");
    }
    
    public static Timestamp ResolveTimestamp(params DateTime?[] values)
    {
        foreach (DateTime? value in values)
        {
            if (value is { } timestamp)
                return new Timestamp(timestamp);
        }

        throw new InvalidOperationException("None of the provided timestamps were set.");
    }
    
    public static bool IsOtherNewer<TSelf, TId>(ITrivyReport<TSelf, TId>? other, Timestamp currentLastSeen)
    where TSelf : ITrivyReport<TSelf, TId>
        => other?.LastSeenAt > currentLastSeen;

    private static IReadOnlyList<OwnerReference> ToOwnerReferences(this V1ObjectMeta metadata)
    {
        if (metadata.OwnerReferences is null)
            return [];

        return
        [
            .. metadata.OwnerReferences.Select(x => new OwnerReference(
                    new Uid(x.Uid),
                    new Kind(x.Kind),
                    new ResourceName(x.Name)
                )
            ),
        ];
    }
}
