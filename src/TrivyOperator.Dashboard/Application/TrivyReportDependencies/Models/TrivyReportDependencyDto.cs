using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Domain.Trivy.ConfigAuditReport;
using TrivyOperator.Dashboard.Domain.Trivy.ExposedSecretReport;
using TrivyOperator.Dashboard.Domain.Trivy.SbomReport;
using TrivyOperator.Dashboard.Domain.Trivy.VulnerabilityReport;
using TrivyOperator.Dashboard.Domain.Utils;

namespace TrivyOperator.Dashboard.Application.TrivyReportDependencies.Models;

public class TrivyReportDependencyDto
{
    public TrivyReportImageDto Image { get; set; } = new();
    public TrivyReportDependencyKubernetesResourceLinkDto[] KubernetesDependencies { get; set; } = [];
}

public class TrivyReportImageDto
{
    public Guid Id => GuidUtils.GetDeterministicGuid(NamespaceName, ImageDigest, ImageName, ImageTag, ImageRepository);
    public string NamespaceName { get; set; } = string.Empty;
    public string ImageDigest { get; set; } = string.Empty;
    public string ImageName { get; set; } = string.Empty;
    public string ImageTag { get; set; } = string.Empty;
    public string ImageRepository { get; set; } = string.Empty;
}

public class TrivyReportDependencyKubernetesResourceLinkDto
{
    public TrivyReportDependencyKubernetesResourceDto KubernetesResource { get; set; } = new();
    public TrivyReportDependencyDetailDto[] TrivyReportDependencies { get; set; } = [];
}

public class TrivyReportDependencyKubernetesResourceBindingDto
{
    public TrivyReportDependencyKubernetesResourceDto KubernetesResource { get; set; } = new();
    public TrivyReportDependencyDetailDto TrivyReportDependency { get; set; } = new();
}

public class TrivyReportDependencyKubernetesResourceDto : IEquatable<TrivyReportDependencyKubernetesResourceDto>
{
    public Guid Id => GuidUtils.GetDeterministicGuid(ResourceContainerName, ResourceKind, ResourceName);
    public string ResourceContainerName { get; set; } = string.Empty;
    public string ResourceKind { get; set; } = string.Empty;
    public string ResourceName { get; set; } = string.Empty;

    public bool Equals(TrivyReportDependencyKubernetesResourceDto? other)
    {
        if (other is null) return false;

        return ResourceContainerName == other.ResourceContainerName 
            && ResourceKind == other.ResourceKind
            && ResourceName == other.ResourceName;
    }

    public override bool Equals(object? obj) => Equals(obj as TrivyReportDependencyKubernetesResourceDto);

    public override int GetHashCode() =>
        HashCode.Combine(ResourceContainerName, ResourceKind, ResourceName);
}

public class TrivyReportDependencyDetailDto
{
    public string Uid { get; init; } = Guid.NewGuid().ToString();
    public TrivyReport TrivyReport { get; set; } = TrivyReport.Unknown;
    public long CriticalCount { get; set; } = -1;
    public long HighCount { get; set; } = -1;
    public long MediumCount { get; set; } = -1;
    public long LowCount { get; set; } = -1;
    public long UnknownCount { get; set; } = -1;
}

public enum TrivyReport
{
    ConfigAudit,
    ExposedSecret,
    Sbom,
    Vulnerability,
    Unknown
}

public static class TrivyReportDependencyDtoExtensions
{
    public static TrivyReportDependencyKubernetesResourceBindingDto ToTrivyReportDependencyKubernetesResourceBindingDto(
        this ConfigAuditReportCr tr)
    {
        return new TrivyReportDependencyKubernetesResourceBindingDto
        {
            KubernetesResource = GetTrivyReportDependencyKubernetesResourceDto(tr),
            TrivyReportDependency = new TrivyReportDependencyDetailDto
            {
                Uid = tr.Uid(),
                TrivyReport = TrivyReport.ConfigAudit,
                CriticalCount = tr.Report?.Summary?.CriticalCount ?? -1,
                HighCount = tr.Report?.Summary?.HighCount ?? -1,
                MediumCount = tr.Report?.Summary?.MediumCount ?? -1,
                LowCount = tr.Report?.Summary?.LowCount ?? -1,
            },
        };
    }

    public static TrivyReportDependencyKubernetesResourceBindingDto ToTrivyReportDependencyKubernetesResourceBindingDto(
        this ExposedSecretReportCr tr)
    {
        return new TrivyReportDependencyKubernetesResourceBindingDto
        {
            KubernetesResource = GetTrivyReportDependencyKubernetesResourceDto(tr),
            TrivyReportDependency = new TrivyReportDependencyDetailDto
            {
                Uid = tr.Uid(),
                TrivyReport = TrivyReport.ExposedSecret,
                CriticalCount = tr.Report?.Summary?.CriticalCount ?? -1,
                HighCount = tr.Report?.Summary?.HighCount ?? -1,
                MediumCount = tr.Report?.Summary?.MediumCount ?? -1,
                LowCount = tr.Report?.Summary?.LowCount ?? -1,
            },
        };
    }

    public static TrivyReportDependencyKubernetesResourceBindingDto ToTrivyReportDependencyKubernetesResourceBindingDto(
        this VulnerabilityReportCr tr)
    {
        return new TrivyReportDependencyKubernetesResourceBindingDto
        {
            KubernetesResource = GetTrivyReportDependencyKubernetesResourceDto(tr),
            TrivyReportDependency = new TrivyReportDependencyDetailDto
            {
                Uid = tr.Uid(),
                TrivyReport = TrivyReport.Vulnerability,
                CriticalCount = tr.Report?.Summary?.CriticalCount ?? -1,
                HighCount = tr.Report?.Summary?.HighCount ?? -1,
                MediumCount = tr.Report?.Summary?.MediumCount ?? -1,
                LowCount = tr.Report?.Summary?.LowCount ?? -1,
                UnknownCount = tr.Report?.Summary?.UnknownCount ?? -1,
            },
        };
    }

    public static TrivyReportDependencyKubernetesResourceBindingDto ToTrivyReportDependencyKubernetesResourceBindingDto(
        this SbomReportCr tr)
    {
        return new TrivyReportDependencyKubernetesResourceBindingDto
        {
            KubernetesResource = GetTrivyReportDependencyKubernetesResourceDto(tr),
            TrivyReportDependency = new TrivyReportDependencyDetailDto
            {
                Uid = tr.Uid(),
                TrivyReport = TrivyReport.Sbom,
            },
        };
    }

    private static TrivyReportDependencyKubernetesResourceDto GetTrivyReportDependencyKubernetesResourceDto(
        IKubernetesObject<V1ObjectMeta> report)
    {
        return new TrivyReportDependencyKubernetesResourceDto
        {
            ResourceKind =
                report.Metadata.Labels != null &&
                report.Metadata.Labels.TryGetValue(
                    "trivy-operator.resource.kind",
                    out string? resourceKind)
                    ? resourceKind
                    : NotAvailable,
            ResourceName =
                report.Metadata.Labels != null &&
                report.Metadata.Labels.TryGetValue(
                    "trivy-operator.resource.name",
                    out string? resourceName)
                    ? resourceName
                    : NotAvailable,
            ResourceContainerName =
                report.Metadata.Labels != null &&
                report.Metadata.Labels.TryGetValue(
                    "trivy-operator.container.name",
                    out string? resourceContainerName)
                    ? resourceContainerName
                    : NotAvailable,
        };
    }

    public static readonly string NotAvailable = "N/A";
}
