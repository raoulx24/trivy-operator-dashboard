using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.SecurityAssessments;

public sealed record Check(
    Category Category,
    CheckId CheckId,
    Description Description,
    IReadOnlyList<string> Messages,
    Remediation Remediation,
    Severity Severity,
    bool Success,
    Title Title
);
