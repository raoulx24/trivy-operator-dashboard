using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ClusterCompliance;

public sealed record Control(
    ControlId Id,
    ControlName ControlName,
    ControlDescription Description,
    Severity Severity,
    IReadOnlyCollection<ControlCheckId> Checks,
    IReadOnlyCollection<ControlCommandId> Commands
);
