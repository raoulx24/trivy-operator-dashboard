using TrivyOperator.Dashboard.Application.Queries.Shared.Identity;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Sboms;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Vulnerabilities;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Mappers;

public static class TrivyReportMappings
{
    internal static VulnerabilityReportDetailDto ToDto(
        this Vulnerability vulnerability)
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
    
    internal static SbomReportDetailDto ToDto(this Component component, SeverityCounters? severityCounters)
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
            Properties: component.Properties,
            Licenses:
            [
                .. component.Licenses.Select(static x
                    => new SbomReportLicenseDto(Id: x.Id, Name: x.Name, Url: x.Url)),
            ],
            CriticalCount: severityCounters?.CriticalCount ?? -1,
            HighCount: severityCounters?.HighCount ?? -1,
            MediumCount: severityCounters?.MediumCount ?? -1,
            LowCount: severityCounters?.LowCount ?? -1,
            UnknownCount: severityCounters?.CriticalCount ?? -1,
            BomRef: component.Id.ToDtoBomRef(),
            DependsOn: [.. component.DependsOnIds.Select(ToDtoBomRef),]
        );
    }
    
    internal static string ToDtoBomRef(this ComponentId value)
    {
        return Guid.TryParse(value.Value, out _) 
            ? value.Value
            : DeterministicId.Create(value.Value).ToString();
    }
    
    internal static SecurityAssessmentReportDetailDto ToDto(
        this Domain.Trivy.ValueObjects.SecurityAssessments.Check check)
    {
        return new SecurityAssessmentReportDetailDto(
            Id: check.CheckId.Value,
            MatchKey: $"{check.Severity.Rank}:{check.CheckId.Value}",
            Category: check.Category.Value,
            CheckId: check.CheckId.Value,
            Description: check.Description.Value,
            Messages: check.Messages,
            Remediation: check.Remediation.Value,
            SeverityId: check.Severity.Rank,
            Success: check.Success,
            Title: check.Title.Value
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
    
    internal static TrivyReportImageInfoDto ToImageInfoDto(
        this ReportImageOccurrence occurrence)
    {
        return new TrivyReportImageInfoDto(
            NameAndTag: $"{occurrence.ImageMeta.Registry.Value}:{occurrence.ImageMeta.Tag.Value}",
            Repository: occurrence.ImageMeta.Repo.Value
        );
    }

    internal static TrivyReportResourceInfoDto ToResourceInfoDto(
        this ReportImageOccurrence occurrence)
    {
        return new TrivyReportResourceInfoDto(
            Name: occurrence.Metadata.GetResourceName().Value,
            Kind: occurrence.Metadata.GetResourceKind().Value,
            ContainerName: occurrence.Container.Value ?? string.Empty
        );
    }
}
