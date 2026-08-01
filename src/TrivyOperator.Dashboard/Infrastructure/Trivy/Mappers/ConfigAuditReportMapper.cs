using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Abstract;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Extensions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ConfigAuditReports;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers;

public class ConfigAuditReportMapper :
    ITrivyReportMapper<ConfigAuditReportCr, ConfigAuditReport>,
    ITrivyReportKeyProvider<ConfigAuditReportCr, Uid>
{
    public ConfigAuditReport MapToDomain(ConfigAuditReportCr cr, ConfigAuditReport? existing)
    {
        return cr.ToSecurityAssessmentReport(existing);
    }
    
    public Uid GetKey(ConfigAuditReportCr cr) => cr.ToUidKey();
}
