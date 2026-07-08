using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Abstract;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Extensions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.SbomReports;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers;

public class ClusterSbomReportMapper : ITrivyReportMapper<ClusterSbomReportCr, ClusterSbomReport>
{
    public ClusterSbomReport MapToDomain(ClusterSbomReportCr cr, ClusterSbomReport? existing)
    {
        return cr.ToClusterSbom(existing);
    }
}
