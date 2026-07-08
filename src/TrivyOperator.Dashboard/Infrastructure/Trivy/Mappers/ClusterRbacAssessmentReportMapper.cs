using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Abstract;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Extensions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.RbacAssessmentReports;


namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers;

public class ClusterRbacAssessmentReportMapper : ITrivyReportMapper<ClusterRbacAssessmentReportCr, ClusterRbacAssessmentReport>
{
    public ClusterRbacAssessmentReport MapToDomain(ClusterRbacAssessmentReportCr cr, ClusterRbacAssessmentReport? existing)
    {
        return cr.ToSecurityAssessmentReport(existing);
    }
}
