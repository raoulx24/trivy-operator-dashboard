using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.SecurityAssessments;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.Trivy.Entities;

// the stored form of InfraAssessmentReport: the check data lives in the checks store, only the ids stay here
public sealed record InfraAssessmentReportSlim(
    ReportMetadata Metadata,
    Scanner Scanner,
    SeverityCounters SeverityCounters,
    Timestamp LastSeenAt,
    IReadOnlyList<CheckId> CheckIds)
    : IResourceReport<InfraAssessmentReportSlim>, ISecurityAssessmentSlimReport<InfraAssessmentReportSlim, InfraAssessmentReport, Uid>, IHasSeverityCounters, INamespacedTrivyReport
{
    public Uid Id => Metadata.Uid;
    public bool HasNamespaceName(NamespaceName namespaceName) => Metadata.NamespaceName == namespaceName;

    public InfraAssessmentReport Hydrate(IReadOnlyList<Check> checks)
        => new(Metadata, Scanner, SeverityCounters, LastSeenAt, checks);

    public InfraAssessmentReportSlim MergeFrom(InfraAssessmentReportSlim other)
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

    public bool IsOtherNewer(InfraAssessmentReportSlim other)
        => other.LastSeenAt >= LastSeenAt;
}
