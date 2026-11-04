using TrivyOperator.Dashboard.Domain.Shared.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.SecurityAssessments;

public sealed record Check(
    Category Category,
    CheckId CheckId,
    Description Description,
    Remediation Remediation,
    Severity Severity,
    Title Title
) : IDetail<CheckId>
{
    public CheckId Id => CheckId;
}
