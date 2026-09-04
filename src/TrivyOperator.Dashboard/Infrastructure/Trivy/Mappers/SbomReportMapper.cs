using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Infrastructure.K8s.Mappers.Abstract;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Extensions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.SbomReports;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers;

public class SbomReportMapper :
    IResourceMapper<SbomReportCr, SbomReport>,
    IResourceKeyProvider<SbomReportCr, Digest>
{
    public SbomReport MapToDomain(SbomReportCr cr, SbomReport? existing)
    {
        return cr.ToSbom(existing);
    }
    
    public Digest GetKey(SbomReportCr kubernetesResource) => kubernetesResource.ToDigestKey();
}
