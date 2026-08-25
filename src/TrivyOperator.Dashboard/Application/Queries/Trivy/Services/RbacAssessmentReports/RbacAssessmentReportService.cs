using TrivyOperator.Dashboard.Application.Queries.Trivy.Mappers;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.RbacAssessmentReports.Abstractions;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Services.RbacAssessmentReports;

public class RbacAssessmentReportService(
    IResourceProvider<RbacAssessmentReport, Uid> resourceProvider
) : IRbacAssessmentReportService
{
    public async Task<IEnumerable<RbacAssessmentReportDto>> GetRbacAssessmentReportDtos(
        string? namespaceName = null,
        IReadOnlySet<int>? includedSeverityIds = null,
        CancellationToken ctx = default)
    {
        IReadOnlyList<RbacAssessmentReport> reports;
        if (includedSeverityIds is null)
        {
            reports = await resourceProvider.GetResources(ctx);
        }
        else
        {
            IReadOnlyList<RbacAssessmentReport> summaries = await resourceProvider.GetResourceSummaries(ctx);
            
            IReadOnlyList<Uid> ids =
            [
                .. summaries
                    .Where(x => x.SeverityCounters.HasAnyOf(includedSeverityIds))
                    .Select(x => x.Id),
            ];
            
            reports = await resourceProvider.GetResources(ids, ctx); 
        }

        return reports.Select(static x => x.ToDto());
    }

    public async Task<IEnumerable<RbacAssessmentReportDenormalizedDto>>
        GetRbacAssessmentReportDenormalizedDtos(
            string? namespaceName = null,
            CancellationToken ctx = default)
    {
        IReadOnlyList<RbacAssessmentReport> reports =
            await resourceProvider.GetResourceSummaries(ctx);

        return reports
            .Where(report =>
                string.IsNullOrEmpty(namespaceName) ||
                report.Resource.NamespaceName.ToString() == namespaceName)
            .SelectMany(report => report.ToDenormalizedDtos());
    }
}
