using TrivyOperator.Dashboard.Domain.TrivyOld.CustomResources.Abstractions;

namespace TrivyOperator.Dashboard.Domain.TrivyOld.RbacAssessmentReport;

public class RbacAssessmentReportCrd : CustomResourceDefinition
{
    public override string Version => "v1alpha1";
    public override string Group => "aquasecurity.github.io";
    public override string PluralName => "rbacassessmentreports";
    public override string Kind => "CResource";
    public override string? Namespace { get; init; } = "default";
}
