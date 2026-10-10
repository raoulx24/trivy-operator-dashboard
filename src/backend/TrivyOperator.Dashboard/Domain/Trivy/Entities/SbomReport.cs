using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Sboms;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.Trivy.Entities;

public sealed record SbomReport(
    IReadOnlyList<ReportImageOccurrence> Occurrences,
    Digest ImageDigest,
    
    Timestamp LastSeenAt,
    
    Scanner Scanner,
    SbomSummary Summary,
    SbomMetadata SbomMetadata,
    ComponentId RootNodeBomRef,
    
    IReadOnlyList<Component> Components) 
    : IImageReport<SbomReport>, ISbomReport<SbomReport, Digest>, INamespacedTrivyReport
{
    public Digest Id => ImageDigest;
    public bool HasNamespaceName(NamespaceName namespaceName)
        => Occurrences.Any(x => x.Metadata.NamespaceName == namespaceName);

    public SbomReport WithOccurrences(
        IReadOnlyList<ReportImageOccurrence> occurrences)
        => this with { Occurrences = occurrences };
    public SbomReport WithComponents(IReadOnlyList<Component> components)
        => this with { Components = components, };
    
    public SbomReport MergeFrom(SbomReport other)
    {
        if (ImageDigest != other.ImageDigest)
            return this;

        bool otherIsNewer = IsOtherNewer(other);

        return this with
        {
            Occurrences = Occurrences.MergeInto(other.Occurrences),
            LastSeenAt = otherIsNewer ? other.LastSeenAt : LastSeenAt,
            Scanner = otherIsNewer ? other.Scanner : Scanner,
            Summary = otherIsNewer ? other.Summary : Summary,
            SbomMetadata = otherIsNewer ? other.SbomMetadata : SbomMetadata,
            Components = otherIsNewer ? other.Components : Components,
        };
    }
    
    public bool IsOtherNewer(SbomReport other)
        => other.LastSeenAt >= LastSeenAt;
}
