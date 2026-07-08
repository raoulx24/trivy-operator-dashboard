using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Infrastructure.K8s.CustomResources;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Abstract;

public interface ITrivyReportMapper<in TTrivyReportCr, TTrivyReport>
where TTrivyReportCr : CustomResource
where TTrivyReport : ITrivyReport
{
    TTrivyReport MapToDomain(TTrivyReportCr cr, TTrivyReport? existing);
}
