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
    Summary Summary,
    
    IReadOnlyList<Secret> Secrets
) : IImageReport<ExposedSecretReport>
{
    public Digest Id => ImageDigest;
    
    public ExposedSecretReport WithOccurrences(
        IReadOnlyList<ReportImageOccurrence> occurrences)
        => this with { Occurrences = occurrences };
}