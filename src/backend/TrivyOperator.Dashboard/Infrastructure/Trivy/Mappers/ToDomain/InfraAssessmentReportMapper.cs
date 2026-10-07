using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Mappers.Abstract;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.ToDomain.Extensions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.InfraAssessmentReports;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.ToDomain;

public class InfraAssessmentReportMapper : 
    IResourceMapper<InfraAssessmentReportCr, InfraAssessmentReport>,
    IResourceKeyProvider<InfraAssessmentReportCr, Uid>
{
    public InfraAssessmentReport MapToDomain(InfraAssessmentReportCr cr, InfraAssessmentReport? existing)
    {
        return cr.ToSecurityAssessmentReport<InfraAssessmentReportCr, InfraAssessmentReport, Uid>(existing);
    }
    
    public Uid GetKey(InfraAssessmentReportCr kubernetesResource) => kubernetesResource.ToUidKey();
}
