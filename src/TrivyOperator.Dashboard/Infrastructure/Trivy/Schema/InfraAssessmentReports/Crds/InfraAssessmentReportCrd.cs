using TrivyOperator.Dashboard.Infrastructure.K8s.CustomResources;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.InfraAssessmentReports.Crds;

public class InfraAssessmentReportCrd : CustomResourceDefinition
{
    public override string Version => "v1alpha1";
    public override string Group => "aquasecurity.github.io";
    public override string PluralName => "infraassessmentreports";
    public override string Kind => "CResource";
    public override string? Namespace { get; init; } = "default";
}
