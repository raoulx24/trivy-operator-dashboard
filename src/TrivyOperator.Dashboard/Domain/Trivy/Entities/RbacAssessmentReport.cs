using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.SecurityAssessments;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.Trivy.Entities;

public sealed record RbacAssessmentReport(
    ReportMetadata Metadata,
    Resource Resource,
    Scanner Scanner,
    Summary Summary,
    Timestamp LastSeenAt,
    IReadOnlyList<Check> Checks)
    : IResourceReport, ISecurityAssessmentReport<RbacAssessmentReport, Uid>
{
    public Uid Id => Metadata.Uid;
    public RbacAssessmentReport WithChecks(IReadOnlyList<Check> checks)
        => this with { Checks = checks, };
}
