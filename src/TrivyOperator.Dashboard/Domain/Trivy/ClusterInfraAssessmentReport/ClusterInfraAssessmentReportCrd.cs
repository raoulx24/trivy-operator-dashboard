using TrivyOperator.Dashboard.Domain.Trivy.CustomResources.Abstractions;

namespace TrivyOperator.Dashboard.Domain.Trivy.ClusterInfraAssessmentReport;

public class ClusterInfraAssessmentReportCrd : CustomResourceDefinition
{
    public override string Version => "v1alpha1";
    public override string Group => "aquasecurity.github.io";
    public override string PluralName => "clusterinfraassessmentreports";
    public override string Kind => "CResource";
    public override string? Namespace { get; init; } = null;
}
