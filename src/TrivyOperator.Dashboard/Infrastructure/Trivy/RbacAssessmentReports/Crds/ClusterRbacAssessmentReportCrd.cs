
using TrivyOperator.Dashboard.Infrastructure.K8s.CustomResources;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.RbacAssessmentReports.Crds;

public class ClusterRbacAssessmentReportCrd : CustomResourceDefinition
{
    public override string Version => "v1alpha1";
    public override string Group => "aquasecurity.github.io";
    public override string PluralName => "clusterrbacassessmentreports";
    public override string Kind => "CResource";
    public override string? Namespace { get; init; } = null;
}
