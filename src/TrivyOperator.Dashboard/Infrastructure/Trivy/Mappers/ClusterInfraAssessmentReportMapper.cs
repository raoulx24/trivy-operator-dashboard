using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Abstract;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Extensions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.InfraAssessmentReports;


namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers;

public class ClusterInfraAssessmentReportMapper : ITrivyReportMapper<ClusterInfraAssessmentReportCr, ClusterInfraAssessmentReport>
{
    public ClusterInfraAssessmentReport MapToDomain(ClusterInfraAssessmentReportCr cr, ClusterInfraAssessmentReport? existing)
    {
        return cr.ToSecurityAssessmentReport(existing);
    }
}
