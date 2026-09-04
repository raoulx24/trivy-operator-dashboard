using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Infrastructure.K8s.Mappers.Abstract;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Extensions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ExposedSecretReports;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers;

public class ExposedSecretReportMapper : 
    IResourceMapper<ExposedSecretReportCr, ExposedSecretReport>,
    IResourceKeyProvider<ExposedSecretReportCr, Digest>
{
    public ExposedSecretReport MapToDomain(ExposedSecretReportCr cr, ExposedSecretReport? existing)
    {
        return cr.ToVExposedSecretReport(existing);
    }
    
    public Digest GetKey(ExposedSecretReportCr kubernetesResource) => kubernetesResource.ToDigestKey();
}
