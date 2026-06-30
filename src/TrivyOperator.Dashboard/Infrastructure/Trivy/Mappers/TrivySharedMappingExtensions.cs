using k8s.Models;
using TrivyOperator.Dashboard.Domain.History.NamespaceHistory.ValueObjects;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.ValueObjects;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Shared;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers;

public static class TrivySharedMappingExtensions
{
    public static ReportMetadata ToReportMetadata(this V1ObjectMeta metadata)
    {
        return new ReportMetadata(
            new ResourceName(metadata.Name),
            new NamespaceName(metadata.NamespaceProperty),
            new Timestamp(metadata.CreationTimestamp ?? DateTime.MinValue),
            Guid.TryParse(metadata.Uid, out Guid g) ? g : Guid.Empty);
    }    
    
    public static Resource ToResource(this V1ObjectMeta metadata)
    {
        IDictionary<string, string>? labels = metadata.Labels;

        return new Resource(
            new ResourceName(GetResourceName("trivy-operator.resource.name")),
            new Kind(Get("trivy-operator.resource.kind")),
            new NamespaceName(Get("trivy-operator.resource.namespace")),
            new ContainerName(Get("trivy-operator.container.name")));

        string GetResourceName(string key)
            => metadata.OwnerReferences?
                   .FirstOrDefault(x => x.Controller == true)?
                   .Name
               ?? Get(key);
        
        string Get(string key)
            => labels != null && labels.TryGetValue(key, out string? v)
                ? v
                : string.Empty;
    }
    
    public static ImageMeta ToImageMeta(ArtifactCr artifact, RegistryCr registry)
    {
        return new ImageMeta(
            new ImageRegistry(registry.Server),
            new ImageRepository(artifact.Repository),
            new ImageTag(artifact.Tag));
    }
    
    public static Digest ToDigest(ArtifactCr artifact)
    {
        return new Digest(artifact.Digest);
    }

    public static Scanner ToScanner(ScannerCr scanner)
    {
        return new Scanner(
            new ScannerName(scanner.Name),
            new ScannerVendor(scanner.Vendor),
            new ScannerVersion(scanner.Version));
    }
    
    public static Timestamp ResolveTimestamp(params string?[] values)
    {
        foreach (var value in values)
        {
            if (DateTime.TryParse(value, out var timestamp))
                return new Timestamp(timestamp);
        }

        throw new InvalidOperationException("None of the provided timestamps could be parsed.");
    }
    
    public static Timestamp ResolveTimestamp(params DateTime?[] values)
    {
        foreach (var value in values)
        {
            if (value is { } timestamp)
                return new Timestamp(timestamp);
        }

        throw new InvalidOperationException("None of the provided timestamps were set.");
    }
}
