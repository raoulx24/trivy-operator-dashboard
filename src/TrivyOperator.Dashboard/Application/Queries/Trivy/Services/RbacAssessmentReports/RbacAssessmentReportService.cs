using TrivyOperator.Dashboard.Application.Queries.Shared;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Mappers;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.RbacAssessmentReports.Abstractions;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Shared;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Services.RbacAssessmentReports;

public class RbacAssessmentReportService(
    IResourceProvider<RbacAssessmentReport, Uid> resourceProvider
) : IRbacAssessmentReportService
{
    public async Task<QueryResponse<IEnumerable<RbacAssessmentReportDto>>> GetRbacAssessmentReportDtos(
        string? namespaceName = null,
        string?  excludedSeverities = null,
        CancellationToken ctx = default)
    {
        QueryResponse<IReadOnlyList<RbacAssessmentReport>> result = await TrivyQuerySupport.GetResources(resourceProvider, namespaceName, excludedSeverities, ctx);

        return new QueryResponse<IEnumerable<RbacAssessmentReportDto>>(
            result.Payload.Select(static x => x.ToDto()),
            result.Error);
    }
    
    public async Task<RbacAssessmentReportDto?> GetRbacAssessmentReportDtoByUid(
        string uid,
        CancellationToken ctx = default)
    {
        RbacAssessmentReport? report = await resourceProvider.GetResource(new Uid(uid), ctx);
        
        return report?.ToDto();
    }

    public async Task<IEnumerable<RbacAssessmentReportDenormalizedDto>>
        GetRbacAssessmentReportDenormalizedDtos(
            string? namespaceName = null,
            CancellationToken ctx = default)
    {
        IReadOnlyList<RbacAssessmentReport> result = await TrivyQuerySupport.GetResources(
            resourceProvider, 
            namespaceName,
            ctx);

        return result.SelectMany(report => report.ToDenormalizedDtos());
    }
}
