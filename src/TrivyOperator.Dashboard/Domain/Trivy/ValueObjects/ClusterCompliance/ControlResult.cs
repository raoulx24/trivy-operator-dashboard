namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ClusterCompliance;

public sealed record ControlResult(
    Control Control,
    CheckResultTotalFail TotalFail
);
