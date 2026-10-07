using TrivyOperator.Dashboard.Application.Shared.Queries;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Mappers;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.RbacAssessmentReports.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Shared;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;

namespace TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.RbacAssessmentReports;

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
    
    public async Task<string[]> GetActiveNamespaces(CancellationToken ctx = default)
    {
        IReadOnlySet<string> result = await TrivyQuerySupport.GetActiveNamespaces(resourceProvider, ctx);

        return [.. result,];
    }
}
