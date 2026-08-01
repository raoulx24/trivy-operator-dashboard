using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
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
) : IResourceReport
{
    public Uid Id => Occurrence.Metadata.Uid;
    public ReportMetadata Metadata => Occurrence.Metadata;
}
