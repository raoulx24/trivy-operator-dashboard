using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Abstract;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Extensions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.InfraAssessmentReports;


namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers;

public class ClusterInfraAssessmentReportMapper :
    ITrivyReportMapper<ClusterInfraAssessmentReportCr, ClusterInfraAssessmentReport>,
    ITrivyReportKeyProvider<ClusterInfraAssessmentReportCr, Uid>
{
    public ClusterInfraAssessmentReport MapToDomain(ClusterInfraAssessmentReportCr cr, ClusterInfraAssessmentReport? existing)
    {
        return cr.ToSecurityAssessmentReport<ClusterInfraAssessmentReportCr, ClusterInfraAssessmentReport, Uid>(existing);
    }
    
    public Uid GetKey(ClusterInfraAssessmentReportCr cr) => cr.ToUidKey();
}
