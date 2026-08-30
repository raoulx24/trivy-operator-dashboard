using TrivyOperator.Dashboard.Application.Queries.Trivy.Mappers;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ClusterRbacAssessmentReports.Abstractions;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ClusterRbacAssessmentReports;

public class ClusterRbacAssessmentReportService(
    IResourceProvider<ClusterRbacAssessmentReport, Uid> resourceProvider
) : IClusterRbacAssessmentReportService
{
    public async Task<ClusterRbacAssessmentReportDto?> GetClusterRbacAssessmentReportDtoByUid(
        string uid,
        CancellationToken ctx = default)
    {
        ClusterRbacAssessmentReport? report = await resourceProvider.GetResource(new Uid(uid), ctx);
        
        return report?.ToDto();
    }

    public async Task<IEnumerable<ClusterRbacAssessmentReportDto>> GetClusterRbacAssessmentReportDtos(
        CancellationToken ctx = default)
    {
        IReadOnlyList<ClusterRbacAssessmentReport> reports =
            await resourceProvider.GetResourceSummaries(ctx);

        return reports.Select(x => x.ToDto());
    }

    public async Task<IEnumerable<ClusterRbacAssessmentReportDenormalizedDto>>
        GetClusterRbacAssessmentReportDenormalizedDtos(
            CancellationToken ctx = default)
    {
        IReadOnlyList<ClusterRbacAssessmentReport> reports =
            await resourceProvider.GetResourceSummaries(ctx);

        return reports.SelectMany(x => x.ToDenormalizedDtos());
    }
}
