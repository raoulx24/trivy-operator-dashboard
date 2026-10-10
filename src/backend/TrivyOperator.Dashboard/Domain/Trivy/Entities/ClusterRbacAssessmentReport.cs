using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.SecurityAssessments;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.Trivy.Entities;

public sealed record ClusterRbacAssessmentReport(
    ReportMetadata Metadata,
    Scanner Scanner,
    SeverityCounters SeverityCounters,
    Timestamp LastSeenAt,
    IReadOnlyList<Check> Checks)
    : IResourceReport<ClusterRbacAssessmentReport>, ISecurityAssessmentReport<ClusterRbacAssessmentReport, Uid>, IHasSeverityCounters, IClusterScopedTrivyReport
{
    public Uid Id => Metadata.Uid;
    public bool HasNamespaceName(NamespaceName namespaceName) => Metadata.NamespaceName == namespaceName;

    public ClusterRbacAssessmentReport WithChecks(IReadOnlyList<Check> checks)
        => this with { Checks = checks, };
    
    public ClusterRbacAssessmentReport MergeFrom(ClusterRbacAssessmentReport other)
    {
        bool otherIsNewer = IsOtherNewer(other);

        return this with
        {
            Metadata = other.Metadata,
            Scanner = otherIsNewer ? other.Scanner : Scanner,
            SeverityCounters = otherIsNewer ? other.SeverityCounters : SeverityCounters,
            LastSeenAt = otherIsNewer ? other.LastSeenAt : LastSeenAt,
            Checks = otherIsNewer ? other.Checks : Checks,
        };
    }
    
    public bool IsOtherNewer(ClusterRbacAssessmentReport other)
        => other.LastSeenAt >= LastSeenAt;
}
