using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ExposedSecrets;

public sealed record Rule(
    Category Category,
    RuleId RuleId,
    Severity Severity,
    Title Title
);
