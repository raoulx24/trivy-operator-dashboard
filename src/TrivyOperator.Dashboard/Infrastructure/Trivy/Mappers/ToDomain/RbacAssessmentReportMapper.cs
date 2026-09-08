using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Mappers.Abstract;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.ToDomain.Extensions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.RbacAssessmentReports;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.ToDomain;

public class RbacAssessmentReportMapper : 
    IResourceMapper<RbacAssessmentReportCr, RbacAssessmentReport>,
    IResourceKeyProvider<RbacAssessmentReportCr, Uid>
{
    public RbacAssessmentReport MapToDomain(RbacAssessmentReportCr cr, RbacAssessmentReport? existing)
    {
        return cr.ToSecurityAssessmentReport<RbacAssessmentReportCr, RbacAssessmentReport, Uid>(existing);
    }
    
    public Uid GetKey(RbacAssessmentReportCr kubernetesResource) => kubernetesResource.ToUidKey();
}
