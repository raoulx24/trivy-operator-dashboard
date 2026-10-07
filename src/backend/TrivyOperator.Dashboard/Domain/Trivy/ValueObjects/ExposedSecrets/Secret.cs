using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ExposedSecrets;

public sealed record Secret(
    Rule Rule,
    Match Match,
    Target Target
);
