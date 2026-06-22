using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.ValueObjects;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.SecurityAssessments;

namespace TrivyOperator.Dashboard.Domain.Trivy.Models.Abstracts;

public abstract record SecurityAssessmentReportBase(
    ReportMetadata Metadata,
    Resource Resource,
    Scanner Scanner,
    Summary Summary,
    Timestamp UpdateTimestamp,
    IReadOnlyList<Check> Checks
);
