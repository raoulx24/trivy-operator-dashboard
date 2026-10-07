using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.SecurityAssessments;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.Abstracts;

public interface ISecurityAssessmentReportCr
{
    ReportCr Report { get; }
}
