using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.ValueObjects;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Models.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.Models.Factories;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Sboms;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.Trivy.Models;

public sealed record SbomReport(
    ReportMetadata Metadata,
    Resource Resource,
    ImageUsage ImageUsage,
    Scanner Scanner,
    Summary Summary,
    SbomMetadata SbomMetadata,
    ComponentId RootNodeBomRef,
    Timestamp LastSeenAt,
    IReadOnlyList<Component> Components)
    : TrivyReportBase(Metadata)
{
    protected override Kind ExpectedKind => ReportKinds.Sbom;
    protected override bool IsClusterScoped => false;
}