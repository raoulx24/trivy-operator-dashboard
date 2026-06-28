using TrivyOperator.Dashboard.Infrastructure.K8s.CustomResources;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ConfigAuditReports.Crds;

public class ClusterConfigAuditReportCrd : CustomResourceDefinition
{
    public override string Version => "v1alpha1";
    public override string Group => "aquasecurity.github.io";
    public override string PluralName => "clusterconfigauditreports";
    public override string Kind => "CResource";
    public override string? Namespace { get; init; } = "default";
}
