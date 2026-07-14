using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ExposedSecrets;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared.Identities;

namespace TrivyOperator.Dashboard.Domain.Trivy.Entities;

public sealed record ExposedSecretReport(
    IReadOnlyList<ReportImageOccurrence> Occurrences,
    NamespaceName NamespaceName,
    Digest ImageDigest,
    
    Timestamp LastSeenAt,
    
    Scanner Scanner,
    Summary Summary,
    
    IReadOnlyList<Secret> Secrets
) : IDigestBasedReport
{
    public NamespacedDigest Id => new(NamespaceName, ImageDigest);
}