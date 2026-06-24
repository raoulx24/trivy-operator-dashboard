using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.ValueObjects;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Models.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.Models.Factories;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.SecurityAssessments;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.Trivy.Models;

public sealed record ClusterConfigAuditReport(
    ReportMetadata Metadata,
    Resource Resource,
    Scanner Scanner,
    Summary Summary,
    Timestamp UpdateTimestamp,
    IReadOnlyList<Check> Checks)
    : TrivyReportBase(Metadata)
{
    protected override Kind ExpectedKind => ReportKinds.ClusterConfigAudit;
    protected override bool IsClusterScoped => true;
}
