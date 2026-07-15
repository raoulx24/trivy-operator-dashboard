using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared.Identities;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Abstract;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Extensions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.RbacAssessmentReports;


namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers;

public class RbacAssessmentReportMapper : 
    ITrivyReportMapper<RbacAssessmentReportCr, RbacAssessmentReport>,
    ITrivyReportKeyProvider<RbacAssessmentReportCr, NamespacedUid>
{
    public RbacAssessmentReport MapToDomain(RbacAssessmentReportCr cr, RbacAssessmentReport? existing)
    {
        return cr.ToSecurityAssessmentReport(existing);
    }
    
    public NamespacedUid GetKey(RbacAssessmentReportCr cr) => cr.ToNamespacedUidKey();
}
