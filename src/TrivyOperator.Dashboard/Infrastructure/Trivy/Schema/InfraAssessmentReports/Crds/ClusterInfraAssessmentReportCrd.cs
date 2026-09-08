using TrivyOperator.Dashboard.Infrastructure.Kubernetes.CustomResources;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.InfraAssessmentReports.Crds;

public class ClusterInfraAssessmentReportCrd : CustomResourceDefinition
{
    public override string Version => "v1alpha1";
    public override string Group => "aquasecurity.github.io";
    public override string PluralName => "clusterinfraassessmentreports";
    public override string Kind => "CResource";
    public override string? Namespace { get; init; } = null;
}
