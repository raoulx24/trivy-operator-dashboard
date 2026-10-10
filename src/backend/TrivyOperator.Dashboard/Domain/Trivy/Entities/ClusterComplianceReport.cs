using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ClusterCompliance;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.Trivy.Entities;

public sealed record ClusterComplianceReport(
    ReportMetadata Metadata,
    
    ComplianceMetadata ComplianceMetadata,
    ComplianceSummary Summary,
    CronSchedule Schedule,
    
    Timestamp LastSeenAt,
    
    IReadOnlyList<ControlResult> ControlChecks
) : IResourceReport<ClusterComplianceReport>, IClusterScopedTrivyReport
{
    public Uid Id => Metadata.Uid;
    public bool HasNamespaceName(NamespaceName namespaceName) => Metadata.NamespaceName == namespaceName;
    
    public ClusterComplianceReport MergeFrom(ClusterComplianceReport other)
    {
        bool otherIsNewer = IsOtherNewer(other);

        return this with
        {
            Metadata = other.Metadata,
            LastSeenAt = otherIsNewer ? other.LastSeenAt : LastSeenAt,
            ComplianceMetadata = otherIsNewer ? other.ComplianceMetadata : ComplianceMetadata,
            Summary = otherIsNewer ? other.Summary : Summary,
            Schedule = otherIsNewer ? other.Schedule : Schedule,
            ControlChecks = otherIsNewer ? other.ControlChecks : ControlChecks,
        };
    }
    
    public bool IsOtherNewer(ClusterComplianceReport other)
        => other.LastSeenAt >= LastSeenAt;
}
