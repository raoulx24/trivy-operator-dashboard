using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared.Identities;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Abstract;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Extensions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ExposedSecretReports;


namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers;

public class ExposedSecretReportMapper : 
    ITrivyReportMapper<ExposedSecretReportCr, ExposedSecretReport>,
    ITrivyReportKeyProvider<ExposedSecretReportCr, NamespacedDigest>
{
    public ExposedSecretReport MapToDomain(ExposedSecretReportCr cr, ExposedSecretReport? existing)
    {
        return cr.ToVExposedSecretReport(existing);
    }
    
    public NamespacedDigest GetKey(ExposedSecretReportCr cr) => cr.ToNamespacedDigestKey();
}
