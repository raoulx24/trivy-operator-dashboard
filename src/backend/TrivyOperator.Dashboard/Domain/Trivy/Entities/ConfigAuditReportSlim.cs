using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.SecurityAssessments;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.Trivy.Entities;

// the stored form of ConfigAuditReport: the check data lives in the checks store, only the ids stay here
public sealed record ConfigAuditReportSlim(
    ReportMetadata Metadata,
    Scanner Scanner,
    SeverityCounters SeverityCounters,
    Timestamp LastSeenAt,
    IReadOnlyList<CheckId> CheckIds)
    : IResourceReport<ConfigAuditReportSlim>, ISecurityAssessmentSlimReport<ConfigAuditReportSlim, ConfigAuditReport, Uid>, IHasSeverityCounters, INamespacedTrivyReport
{
    public Uid Id => Metadata.Uid;
    public bool HasNamespaceName(NamespaceName namespaceName) => Metadata.NamespaceName == namespaceName;

    public ConfigAuditReport Hydrate(IReadOnlyList<Check> checks)
        => new(Metadata, Scanner, SeverityCounters, LastSeenAt, checks);

    public ConfigAuditReportSlim MergeFrom(ConfigAuditReportSlim other)
    {
        bool otherIsNewer = IsOtherNewer(other);

        return this with
        {
            Metadata = other.Metadata,
            Scanner = otherIsNewer ? other.Scanner : Scanner,
            SeverityCounters = otherIsNewer ? other.SeverityCounters : SeverityCounters,
            LastSeenAt = otherIsNewer ? other.LastSeenAt : LastSeenAt,
            CheckIds = otherIsNewer ? other.CheckIds : CheckIds,
        };
    }

    public bool IsOtherNewer(ConfigAuditReportSlim other)
        => other.LastSeenAt >= LastSeenAt;
}
