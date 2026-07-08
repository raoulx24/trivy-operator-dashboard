using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Abstract;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Extensions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.InfraAssessmentReports;


namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers;

public class InfraAssessmentReportMapper : ITrivyReportMapper<InfraAssessmentReportCr, InfraAssessmentReport>
{
    public InfraAssessmentReport MapToDomain(InfraAssessmentReportCr cr, InfraAssessmentReport? existing)
    {
        return cr.ToSecurityAssessmentReport(existing);
    }
}
