using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Sboms;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Vulnerabilities;
using TrivyOperator.Dashboard.Domain.Utils;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Mappers;

public static class TrivyReportMappings
{
    public static VulnerabilityReportDetailDto ToDto(
        this Vulnerability vulnerability)
    {
        Uid key = new Uid(GuidUtils.GetDeterministicGuid(
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
    
    public static SbomReportDetailDto ToDto(this Component component, SeverityCounters? severityCounters)
    {
        string id = GuidUtils.GetDeterministicGuid(
                component.Purl?.Value ?? component.Name.Value,
                component.Version.Value
            )
            .ToString();

        return new SbomReportDetailDto(
            Id: id,
            MatchKey: GuidUtils.GetDeterministicGuid(component.Purl?.Value ?? component.Name.Value).ToString(),
            Name: component.Name.Value,
            Purl: component.Purl?.Value ?? string.Empty,
            Version: component.Version.Value,
            Properties: component.Properties,
            Licenses:
            [
                .. component.Licenses.Select(static x => new SbomReportLicenseDto(Id: x.Id, Name: x.Name, Url: x.Url))
            ],
            CriticalCount: severityCounters?.CriticalCount ?? -1,
            HighCount: severityCounters?.HighCount ?? -1,
            MediumCount: severityCounters?.MediumCount ?? -1,
            LowCount: severityCounters?.LowCount ?? -1,
            UnknownCount: severityCounters?.CriticalCount ?? -1,
            BomRef: ToDtoBomRef(component.Id),
            DependsOn:
            [
                .. component.DependsOnIds.Select(ToDtoBomRef),
            ]
        );
    }
    
    public static string ToDtoBomRef(this ComponentId value)
    {
        return Guid.TryParse(value.Value, out _) 
            ? value.Value
            : GuidUtils.GetDeterministicGuid(value.Value).ToString();
    }
}
