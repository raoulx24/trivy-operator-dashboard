using TrivyOperator.Dashboard.Infrastructure.Kubernetes.CustomResources;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ConfigAuditReports.Crds;

public class ConfigAuditReportCrd : CustomResourceDefinition
{
    public override string Version => "v1alpha1";
    public override string Group => "aquasecurity.github.io";
    public override string PluralName => "configauditreports";
    public override string Kind => "CResource";
    public override string? Namespace { get; init; } = "default";
}
