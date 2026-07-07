using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ClusterCompliance;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.Trivy.Entities;

public sealed record ClusterComplianceReport(
    ReportMetadata Metadata,
    
    ComplianceMetadata ComplianceMetadata,
    ComplianceSummary Summary,
    CronSchedule Schedule,
    
    Timestamp LastSeenAt,
    
    IReadOnlyList<ControlResult> ControlChecks
) : ITrivyReport
{
    public Uid Id => Metadata.Uid;
}
