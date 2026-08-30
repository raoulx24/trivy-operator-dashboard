using TrivyOperator.Dashboard.Application.Queries.Trivy.Mappers;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ClusterComplianceReports.Abstractions;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
 
namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ClusterComplianceReports;

public class ClusterComplianceReportService(
    IResourceProvider<ClusterComplianceReport, Uid> resourceProvider
) : IClusterComplianceReportService
{
    public async Task<IEnumerable<ClusterComplianceReportDto>> GetClusterComplianceReportDtos(
        CancellationToken ctx = default)
    {
        IReadOnlyList<ClusterComplianceReport> reports =
            await resourceProvider.GetResourceSummaries(ctx);

        return reports.Select(x => x.ToDto());
    }
    
    public async Task<ClusterComplianceReportDto?> GetClusterComplianceReportDtoByUid(
        string uid,
        CancellationToken ctx = default)
    {
        ClusterComplianceReport? report = await resourceProvider.GetResource(new Uid(uid), ctx);

        return report?.ToDto();
    }


    public async Task<IEnumerable<ClusterComplianceReportDenormalizedDto>> GetClusterComplianceReportDenormalizedDtos(
        CancellationToken ctx = default)
    {
        IReadOnlyList<ClusterComplianceReport> reports =
            await resourceProvider.GetResourceSummaries(ctx);

        return reports.SelectMany(x => x.ToDenormalizedDtos());
    }
}
