using TrivyOperator.Dashboard.Domain.TrivyOld.CustomResources.Abstractions;

namespace TrivyOperator.Dashboard.Domain.TrivyOld.ExposedSecretReport;

public class ExposedSecretReportCrd : CustomResourceDefinition
{
    public override string Version => "v1alpha1";
    public override string Group => "aquasecurity.github.io";
    public override string PluralName => "exposedsecretreports";
    public override string Kind => "CResource";
    public override string? Namespace { get; init; } = "default";
}
