using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.ValueObjects;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Factories;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ClusterCompliance;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.Trivy.Entities;

public sealed record ClusterComplianceReport(
    ReportMetadata Metadata,
    
    ComplianceMetadata ComplianceMetadata,
    ComplianceSummary Summary,
    
    Timestamp LastSeenAt,
    
    IReadOnlyList<CheckResult> ControlChecks
) : TrivyReportBase(Metadata)
{
    protected override Kind ExpectedKind => ReportKinds.ClusterCompliance;
    protected override bool IsClusterScoped => true;
}