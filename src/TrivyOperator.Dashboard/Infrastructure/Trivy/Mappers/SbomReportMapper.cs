using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Abstract;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Extensions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.SbomReports;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers;

public class SbomReportMapper : ITrivyReportMapper<SbomReportCr, SbomReport>
{
    public SbomReport MapToDomain(SbomReportCr cr, SbomReport? existing)
    {
        return cr.ToSbom(existing);
    }
}
