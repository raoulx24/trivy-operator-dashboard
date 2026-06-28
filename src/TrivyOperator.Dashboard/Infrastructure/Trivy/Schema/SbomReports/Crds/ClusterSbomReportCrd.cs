using TrivyOperator.Dashboard.Infrastructure.K8s.CustomResources;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.SbomReports.Crds;

public class ClusterSbomReportCrd : CustomResourceDefinition
{
    public override string Version => "v1alpha1";
    public override string Group => "aquasecurity.github.io";
    public override string PluralName => "clustersbomreports";
    public override string Kind => "CResource";
    public override string? Namespace { get; init; } = null;
}
