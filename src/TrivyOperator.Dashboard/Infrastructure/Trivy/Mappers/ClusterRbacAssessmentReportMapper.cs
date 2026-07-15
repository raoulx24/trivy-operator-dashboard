using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Abstract;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Extensions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.RbacAssessmentReports;


namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers;

public class ClusterRbacAssessmentReportMapper : 
    ITrivyReportMapper<ClusterRbacAssessmentReportCr, ClusterRbacAssessmentReport>,
    ITrivyReportKeyProvider<ClusterRbacAssessmentReportCr, Uid>
{
    public ClusterRbacAssessmentReport MapToDomain(ClusterRbacAssessmentReportCr cr, ClusterRbacAssessmentReport? existing)
    {
        return cr.ToSecurityAssessmentReport(existing);
    }
    
    public Uid GetKey(ClusterRbacAssessmentReportCr cr) => cr.ToUidKey();
}
