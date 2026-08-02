using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Abstract;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Extensions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.InfraAssessmentReports;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers;

public class InfraAssessmentReportMapper : 
    ITrivyReportMapper<InfraAssessmentReportCr, InfraAssessmentReport>,
    ITrivyReportKeyProvider<InfraAssessmentReportCr, Uid>
{
    public InfraAssessmentReport MapToDomain(InfraAssessmentReportCr cr, InfraAssessmentReport? existing)
    {
        return cr.ToSecurityAssessmentReport<InfraAssessmentReportCr, InfraAssessmentReport, Uid>(existing);
    }
    
    public Uid GetKey(InfraAssessmentReportCr cr) => cr.ToUidKey();
}
