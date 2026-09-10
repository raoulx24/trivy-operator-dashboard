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
) : IResourceReport, ISbomReport<ClusterSbomReport, Uid>, IClusterScopedTrivyReport
{
    public Uid Id => Occurrence.Metadata.Uid;
    public bool HasNamespaceName(NamespaceName namespaceName) => Occurrence.Metadata.NamespaceName == namespaceName;

    public ReportMetadata Metadata => Occurrence.Metadata;
    public ClusterSbomReport WithComponents(IReadOnlyList<Component> components)
        => this with { Components = components, };
}
