using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Mappers.Abstract;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.ToDomain.Extensions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.RbacAssessmentReports;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.ToDomain;

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
