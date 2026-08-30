using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.SecurityAssessments;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.Trivy.Entities;

public sealed record ConfigAuditReport(
    ReportMetadata Metadata,
    Scanner Scanner,
    SeverityCounters SeverityCounters,
    Timestamp LastSeenAt,
    IReadOnlyList<Check> Checks)
    : IResourceReport, ISecurityAssessmentReport<ConfigAuditReport, Uid>, IHasSeverityCounters
{
    public Uid Id => Metadata.Uid;
    public bool HasNamespaceName(NamespaceName namespaceName) => Metadata.NamespaceName == namespaceName;

    public ConfigAuditReport WithChecks(IReadOnlyList<Check> checks)
        => this with { Checks = checks, };
}
