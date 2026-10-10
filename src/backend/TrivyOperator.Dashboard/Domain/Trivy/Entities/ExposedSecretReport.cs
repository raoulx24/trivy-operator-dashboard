using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
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
    : IImageReport<ExposedSecretReport>, IHasSeverityCounters, INamespacedTrivyReport
{
    public Digest Id => ImageDigest;
    public bool HasNamespaceName(NamespaceName namespaceName)
        => Occurrences.Any(x => x.Metadata.NamespaceName == namespaceName);

    public ExposedSecretReport WithOccurrences(
        IReadOnlyList<ReportImageOccurrence> occurrences)
        => this with { Occurrences = occurrences };
    
    public ExposedSecretReport MergeFrom(ExposedSecretReport other)
    {
        if (ImageDigest != other.ImageDigest)
            return this;

        bool otherIsNewer = IsOtherNewer(other);

        return this with
        {
            Occurrences = Occurrences.MergeInto(other.Occurrences),
            LastSeenAt = otherIsNewer ? other.LastSeenAt : LastSeenAt,
            Scanner = otherIsNewer ? other.Scanner : Scanner,
            SeverityCounters = otherIsNewer ? other.SeverityCounters : SeverityCounters,
            Secrets = otherIsNewer ? other.Secrets : Secrets,
        };
    }
    
    public bool IsOtherNewer(ExposedSecretReport other)
        => other.LastSeenAt >= LastSeenAt;
}