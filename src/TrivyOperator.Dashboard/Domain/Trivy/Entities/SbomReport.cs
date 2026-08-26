using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
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
    
    IReadOnlyList<Component> Components) : IImageReport<SbomReport>, ISbomReport<SbomReport, Digest>
{
    public Digest Id => ImageDigest;
    public bool HasNamespaceName(NamespaceName namespaceName)
        => Occurrences.Any(x => x.Metadata.NamespaceName == namespaceName);

    public SbomReport WithOccurrences(
        IReadOnlyList<ReportImageOccurrence> occurrences)
        => this with { Occurrences = occurrences };
    public SbomReport WithComponents(IReadOnlyList<Component> components)
        => this with { Components = components, };
}
