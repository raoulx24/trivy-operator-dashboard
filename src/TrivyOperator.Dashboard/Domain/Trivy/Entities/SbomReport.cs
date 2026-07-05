using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.ValueObjects;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Sboms;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared.Identities;

namespace TrivyOperator.Dashboard.Domain.Trivy.Entities;

public sealed record SbomReport(
    IReadOnlyList<ReportImageOccurrence> Occurrences,
    NamespaceName NamespaceName,
    Digest ImageDigest,
    
    Timestamp LastSeenAt,
    
    Scanner Scanner,
    Summary Summary,
    SbomMetadata SbomMetadata,
    ComponentId RootNodeBomRef,
    
    IReadOnlyList<Component> Components) : IDigestBasedReport
{
    public NamespacedDigest Id => new(NamespaceName, ImageDigest);
}
