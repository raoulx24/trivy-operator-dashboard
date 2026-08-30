using TrivyOperator.Dashboard.Application.Queries.Shared;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Mappers;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.InfraAssessmentReports.Abstractions;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.Shared;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Services.InfraAssessmentReports;

public sealed class InfraAssessmentReportService(
    IResourceProvider<InfraAssessmentReport, Uid> resourceProvider
) : IInfraAssessmentReportService
{
    public async Task<QueryResponse<IEnumerable<InfraAssessmentReportDto>>> GetInfraAssessmentReportDtos(
        string? namespaceName = null,
        string? excludedSeverities = null,
        CancellationToken ctx = default)
    {
        QueryResponse<IReadOnlyList<InfraAssessmentReport>> result = 
            await TrivyQuerySupport.GetResources(resourceProvider, namespaceName, excludedSeverities, ctx);

        return new QueryResponse<IEnumerable<InfraAssessmentReportDto>>(
            result.Payload.Select(static x => x.ToDto()),
            result.Error);
    }

    public async Task<InfraAssessmentReportDto?> GetInfraAssessmentReportDtoByUid(
        string uid,
        CancellationToken ctx = default)
    {
        InfraAssessmentReport? report =
            await resourceProvider.GetResource(new Uid(uid), ctx);

        return report?.ToDto();
    }

    public async Task<IEnumerable<InfraAssessmentReportDenormalizedDto>>
        GetInfraAssessmentReportDenormalizedDtos(
            string? namespaceName = null,
            CancellationToken ctx = default)
    {
        IReadOnlyList<InfraAssessmentReport> result = 
            await TrivyQuerySupport.GetResources(resourceProvider, namespaceName, ctx);
        
        return result.SelectMany(static report => report.ToDenormalizedDtos());
    }
}
