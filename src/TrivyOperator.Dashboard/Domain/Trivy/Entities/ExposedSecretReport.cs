using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ExposedSecrets;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.Trivy.Entities;

public sealed record ExposedSecretReport(
    IReadOnlyList<ReportImageOccurrence> Occurrences,
    Digest ImageDigest,
    
    Timestamp LastSeenAt,
    
    Scanner Scanner,
    SeverityCounters SeverityCounters,
    
    IReadOnlyList<Secret> Secrets)
    : IImageReport<ExposedSecretReport>, IHasSeverityCounters
{
    public Digest Id => ImageDigest;
    public bool HasNamespaceName(NamespaceName namespaceName)
        => Occurrences.Any(x => x.Metadata.NamespaceName == namespaceName);

    public ExposedSecretReport WithOccurrences(
        IReadOnlyList<ReportImageOccurrence> occurrences)
        => this with { Occurrences = occurrences };
}