using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Infrastructure.K8s.Mappers.Abstract;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Extensions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.RbacAssessmentReports;


namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers;

public class ClusterRbacAssessmentReportMapper : 
    IResourceMapper<ClusterRbacAssessmentReportCr, ClusterRbacAssessmentReport>,
    IResourceKeyProvider<ClusterRbacAssessmentReportCr, Uid>
{
    public ClusterRbacAssessmentReport MapToDomain(ClusterRbacAssessmentReportCr cr, ClusterRbacAssessmentReport? existing)
    {
        return cr.ToSecurityAssessmentReport<ClusterRbacAssessmentReportCr, ClusterRbacAssessmentReport, Uid>(existing);
    }
    
    public Uid GetKey(ClusterRbacAssessmentReportCr kubernetesResource) => kubernetesResource.ToUidKey();
}
