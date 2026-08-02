using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Abstract;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Extensions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.RbacAssessmentReports;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers;

public class RbacAssessmentReportMapper : 
    ITrivyReportMapper<RbacAssessmentReportCr, RbacAssessmentReport>,
    ITrivyReportKeyProvider<RbacAssessmentReportCr, Uid>
{
    public RbacAssessmentReport MapToDomain(RbacAssessmentReportCr cr, RbacAssessmentReport? existing)
    {
        return cr.ToSecurityAssessmentReport<RbacAssessmentReportCr, RbacAssessmentReport, Uid>(existing);
    }
    
    public Uid GetKey(RbacAssessmentReportCr cr) => cr.ToUidKey();
}
