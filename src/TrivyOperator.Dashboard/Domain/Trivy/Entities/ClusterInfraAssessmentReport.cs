using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.SecurityAssessments;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.Trivy.Entities;

public sealed record ClusterInfraAssessmentReport(
    ReportMetadata Metadata,
    Resource Resource,
    Scanner Scanner,
    Summary Summary,
    Timestamp LastSeenAt,
    IReadOnlyList<Check> Checks)
    : IResourceReport, ISecurityAssessmentReport<ClusterInfraAssessmentReport, Uid>
{
    public Uid Id => Metadata.Uid;
    public ClusterInfraAssessmentReport WithChecks(IReadOnlyList<Check> checks)
        => this with { Checks = checks, };
}
