using TrivyOperator.Dashboard.Domain.Trivy.CustomResources.Abstractions;

namespace TrivyOperator.Dashboard.Domain.Trivy.ClusterComplianceReport;

public class ClusterComplianceReportCrd : CustomResourceDefinition
{
    public override string Version => "v1alpha1";
    public override string Group => "aquasecurity.github.io";
    public override string PluralName => "clustercompliancereports";
    public override string Kind => "CResource";
    public override string? Namespace { get; init; } = null;
}
