namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ClusterCompliance;

public sealed record CheckResult(
    Control Control,
    CheckResultTotalFail TotalFail
);
