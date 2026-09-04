using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Infrastructure.K8s.Mappers.Abstract;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Extensions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ConfigAuditReports;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers;

public class ConfigAuditReportMapper :
    IResourceMapper<ConfigAuditReportCr, ConfigAuditReport>,
    IResourceKeyProvider<ConfigAuditReportCr, Uid>
{
    public ConfigAuditReport MapToDomain(ConfigAuditReportCr cr, ConfigAuditReport? existing)
    {
        return cr.ToSecurityAssessmentReport<ConfigAuditReportCr, ConfigAuditReport, Uid>(existing);
    }
    
    public Uid GetKey(ConfigAuditReportCr kubernetesResource) => kubernetesResource.ToUidKey();
}
