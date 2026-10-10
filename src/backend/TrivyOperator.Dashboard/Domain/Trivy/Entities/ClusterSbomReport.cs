using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Sboms;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.Trivy.Entities;

public sealed record ClusterSbomReport(
    ReportImageOccurrence Occurrence,
    Timestamp LastSeenAt,
    Scanner Scanner,
    SbomSummary Summary,
    SbomMetadata SbomMetadata,
    ComponentId RootNodeBomRef,
    IReadOnlyList<Component> Components
) : IResourceReport<ClusterSbomReport>, ISbomReport<ClusterSbomReport, Uid>, IClusterScopedTrivyReport
{
    public Uid Id => Occurrence.Metadata.Uid;
    public bool HasNamespaceName(NamespaceName namespaceName) => Occurrence.Metadata.NamespaceName == namespaceName;

    public ReportMetadata Metadata => Occurrence.Metadata;
    public ClusterSbomReport WithComponents(IReadOnlyList<Component> components)
        => this with { Components = components, };
    
    public ClusterSbomReport MergeFrom(ClusterSbomReport other)
    {
        bool otherIsNewer = IsOtherNewer(other);

        return this with
        {
            Occurrence = otherIsNewer ? other.Occurrence : Occurrence,
            LastSeenAt = otherIsNewer ? other.LastSeenAt : LastSeenAt,
            Scanner = otherIsNewer ? other.Scanner : Scanner,
            Summary = otherIsNewer ? other.Summary : Summary,
            SbomMetadata = otherIsNewer ? other.SbomMetadata : SbomMetadata,
            Components = otherIsNewer ? other.Components : Components,
        };
    }
    
    public bool IsOtherNewer(ClusterSbomReport other)
        => other.LastSeenAt >= LastSeenAt;
}
