using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Models.Abstracts;

namespace TrivyOperator.Dashboard.Domain.Trivy.Models;

public sealed record RbacSecurityAssessmentReport : SecurityAssessmentReportBase
{
    public static readonly Kind ExpectedKind = new("RbacAssessmentReport");

    public RbacSecurityAssessmentReport(SecurityAssessmentReportCore core)
        : base(
            core.Metadata
                .ValidateKind(ExpectedKind)
                .ValidateNamespace(true),
            core.Resource,
            core.Scanner,
            core.Summary,
            core.UpdateTimestamp,
            core.Checks
        )
    { }
}
