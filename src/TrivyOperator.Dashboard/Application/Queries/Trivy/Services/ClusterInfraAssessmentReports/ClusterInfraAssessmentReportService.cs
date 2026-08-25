using TrivyOperator.Dashboard.Application.Queries.Trivy.Mappers;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ClusterInfraAssessmentReports.Abstractions;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ClusterInfraAssessmentReports;

public sealed class ClusterInfraAssessmentReportService(
    IResourceProvider<ClusterInfraAssessmentReport, Uid> resourceProvider
) : IClusterInfraAssessmentReportService
{
    public async Task<IEnumerable<ClusterInfraAssessmentReportDto>> GetClusterInfraAssessmentReportDtos(
        CancellationToken ctx = default)
    {
        IReadOnlyList<ClusterInfraAssessmentReport> reports = await resourceProvider.GetResourceSummaries(ctx);

        return reports.Select(static x => x.ToDto());
    }

    public async Task<ClusterInfraAssessmentReportDto?> GetClusterInfraAssessmentReportDtoByUid(
        string uid,
        CancellationToken ctx = default)
    {
        ClusterInfraAssessmentReport? report = await resourceProvider.GetResource(new Uid(uid), ctx);

        return report?.ToDto();
    }

    public async Task<IEnumerable<ClusterInfraAssessmentReportDenormalizedDto>>
        GetClusterInfraAssessmentReportDenormalizedDtos(CancellationToken ctx = default)
    {
        IReadOnlyList<ClusterInfraAssessmentReport> reports = await resourceProvider.GetResourceSummaries(ctx);

        return reports.SelectMany(static x => x.ToDenormalizedDtos());
    }
}
