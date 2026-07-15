using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared.Identities;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Abstract;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Extensions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ConfigAuditReports;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers;

public class ConfigAuditReportMapper :
    ITrivyReportMapper<ConfigAuditReportCr, ConfigAuditReport>,
    ITrivyReportKeyProvider<ConfigAuditReportCr, NamespacedUid>
{
    public ConfigAuditReport MapToDomain(ConfigAuditReportCr cr, ConfigAuditReport? existing)
    {
        return cr.ToSecurityAssessmentReport(existing);
    }
    
    public NamespacedUid GetKey(ConfigAuditReportCr cr) => cr.ToNamespacedUidKey();
}
