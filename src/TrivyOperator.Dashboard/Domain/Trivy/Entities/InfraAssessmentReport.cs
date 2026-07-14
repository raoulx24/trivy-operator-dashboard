using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.SecurityAssessments;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared.Identities;

namespace TrivyOperator.Dashboard.Domain.Trivy.Entities;

public sealed record InfraAssessmentReport(
    ReportMetadata Metadata,
    Resource Resource,
    Scanner Scanner,
    Summary Summary,
    Timestamp LastSeenAt,
    IReadOnlyList<Check> Checks)
    : INamespaceUidBasedTrivyReport
{
    public NamespacedUid Id => new(Metadata.NamespaceName, Metadata.Uid);
    public NamespaceName NamespaceName => Metadata.NamespaceName;
}
