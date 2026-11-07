using TrivyOperator.Dashboard.Application.Shared.Queries.Identity;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Sboms;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Vulnerabilities;

namespace TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Mappers;

public static class TrivyReportMappings
{
    internal static VulnerabilityReportDetailDto ToDto(this Vulnerability vulnerability)
    {
        Uid key = new(DeterministicId.Create(
            vulnerability.Id.Value, 
            vulnerability.ScannedPackage.Name.Value, 
            vulnerability.ScannedPackage.InstalledVersion.Value, 
            vulnerability.ScannedPackage.Target.Value));
        return new VulnerabilityReportDetailDto(
            Id: key.Value,
            MatchKey: key.Value,
            FixedVersion: vulnerability.FixedVersion?.Value ?? string.Empty,
            InstalledVersion: vulnerability.ScannedPackage.InstalledVersion.Value,
            LastModifiedDate: vulnerability.Modified?.Value,
            PackageUrl: vulnerability.ScannedPackage.Purl.Value,
            PrimaryLink: vulnerability.PrimaryLink.InitialValue,
            PublishedDate: vulnerability.Published?.Value,
            Resource: vulnerability.ScannedPackage.Name.Value,
            Score: vulnerability.Score.Value,
            SeverityId: vulnerability.Severity.Rank,
            Target: vulnerability.ScannedPackage.Target.Value,
            Title: vulnerability.Title.Value,
            VulnerabilityId: vulnerability.Id.Value
        );
    }
    
    internal static SbomReportDetailDto ToDto(
        this Component component,
        SeverityCounters? severityCounters,
        IReadOnlyList<ImageLayer> layers,
        bool isRoot)
    {
        string id = DeterministicId.Create(
                component.Purl?.Value ?? component.Name.Value,
                component.Version.Value
            )
            .ToString();

        return new SbomReportDetailDto(
            Id: id,
            MatchKey: DeterministicId.Create(component.Purl?.Value ?? component.Name.Value).ToString(),
            Name: component.Name.Value,
            Purl: component.Purl?.Value ?? string.Empty,
            Version: component.Version.Value,
            Properties:
            [
                .. component.GetAllProperties(layers, isRoot)
                    .Select(static x => new SbomReportDetailPropertyDto(x.Name, x.Value)),
            ],
            Licenses:
            [
                .. component.Licenses.Select(x => x.Name ?? x.Id ?? string.Empty)
                       .Where(x => !string.IsNullOrWhiteSpace(x)),
            ],
            CriticalCount: severityCounters?.CriticalCount,
            HighCount: severityCounters?.HighCount,
            MediumCount: severityCounters?.MediumCount,
            LowCount: severityCounters?.LowCount,
            UnknownCount: severityCounters?.UnknownCount,
            BomRef: component.Id.ToDtoBomRef(),
            DependsOn: [.. component.DependsOnIds.Select(ToDtoBomRef),]
        );
    }
    
    // Re-expands the layer table into Trivy's original properties: the root gets one DiffID per image layer,
    // a component gets the LayerDiffID/LayerDigest of its layer.
    internal static IEnumerable<ComponentProperty> GetAllProperties(
        this Component component,
        IReadOnlyList<ImageLayer> layers,
        bool isRoot)
    {
        if (isRoot)
        {
            foreach (ImageLayer layer in layers)
            {
                yield return new ComponentProperty(LayerPropertyNames.DiffId, layer.DiffId.Value);
            }
        }

        if (component.LayerIndex is { } layerIndex && layerIndex >= 0 && layerIndex < layers.Count)
        {
            ImageLayer layer = layers[layerIndex];
            yield return new ComponentProperty(LayerPropertyNames.LayerDiffId, layer.DiffId.Value);

            if (layer.Digest.IsValid)
                yield return new ComponentProperty(LayerPropertyNames.LayerDigest, layer.Digest.Value);
        }

        foreach (ComponentProperty property in component.Properties)
        {
            yield return property;
        }
    }

    internal static string ToDtoBomRef(this ComponentId value)
    {
        return Guid.TryParse(value.Value, out _) 
            ? value.Value
            : DeterministicId.Create(value.Value).ToString();
    }
    
    // one row per check, with the number of times it is in the report
    // (a check failing for several containers is in the report several times)
    internal static IEnumerable<SecurityAssessmentReportDetailDto> ToDetailDtos(
        this IEnumerable<Domain.Trivy.ValueObjects.SecurityAssessments.Check> checks)
    {
        return checks
            .GroupBy(static x => x.CheckId)
            .Select(static x => x.First().ToDto(x.Count()));
    }

    private static SecurityAssessmentReportDetailDto ToDto(
        this Domain.Trivy.ValueObjects.SecurityAssessments.Check check,
        int count)
    {
        return new SecurityAssessmentReportDetailDto(
            Id: check.CheckId.Value,
            MatchKey: $"{check.Severity.Rank}:{check.CheckId.Value}",
            Category: check.Category.Value,
            CheckId: check.CheckId.Value,
            Description: check.Description.Value,
            Remediation: check.Remediation.Value,
            SeverityId: check.Severity.Rank,
            Title: check.Title.Value,
            Count: count
        );
    }

    internal static ResourceName GetResourceName(this ReportMetadata metadata)
    {
        IReadOnlyList<OwnerReference>? ownerReferences = metadata.OwnerReferences;

        if (ownerReferences is not null && ownerReferences.Count > 0)
        {
            return ownerReferences[0].Name;
        }

        return metadata.Name;
    }
    
    internal static Kind GetResourceKind(this ReportMetadata metadata)
    {
        IReadOnlyList<OwnerReference>? ownerReferences = metadata.OwnerReferences;

        if (ownerReferences is not null && ownerReferences.Count > 0)
        {
            return ownerReferences[0].Kind;
        }

        return new Kind();
    }
    
    internal static TrivyReportImageInfoDto ToImageInfoDto(this ReportImageOccurrence occurrence)
    {
        return new TrivyReportImageInfoDto(
            NameAndTag: $"{occurrence.ImageMeta.Repo.Value}:{occurrence.ImageMeta.Tag.Value}",
            Registry: occurrence.ImageMeta.Registry.Value
        );
    }

    internal static TrivyReportResourceInfoDto ToResourceInfoDto(this ReportImageOccurrence occurrence)
    {
        return new TrivyReportResourceInfoDto(
            Name: occurrence.Metadata.GetResourceName().Value,
            NamespaceName: occurrence.Metadata.NamespaceName.Value,
            Kind: occurrence.Metadata.GetResourceKind().Value,
            ContainerName: occurrence.Container.Value ?? string.Empty
        );
    }
}
