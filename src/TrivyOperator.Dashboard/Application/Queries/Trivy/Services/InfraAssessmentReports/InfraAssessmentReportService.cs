using TrivyOperator.Dashboard.Application.Queries.Trivy.Mappers;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.InfraAssessmentReports.Abstractions;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Services.InfraAssessmentReports;

public sealed class InfraAssessmentReportService(
    IResourceProvider<InfraAssessmentReport, Uid> resourceProvider
) : IInfraAssessmentReportService
{
    public async Task<IEnumerable<InfraAssessmentReportDto>> GetInfraAssessmentReportDtos(
        string? namespaceName = null,
        IEnumerable<int>? excludedSeverities = null,
        CancellationToken ctx = default)
    {
        IReadOnlyList<InfraAssessmentReport> reports =
            await resourceProvider.GetResourceSummaries(ctx);

        HashSet<int> excludedSeverityIds =
            [.. excludedSeverities ?? []];

        bool hasExcludedSeverities = excludedSeverityIds.Count > 0;

        return reports
            .Where(report =>
                string.IsNullOrEmpty(namespaceName) ||
                report.Resource.NamespaceName.Value == namespaceName)
            .Select(report =>
            {
                InfraAssessmentReportDto dto = report.ToDto();

                if (excludedSeverityIds.Count == 0)
                    return dto;

                IReadOnlyList<SecurityAssessmentReportDetailDto> details =
                [
                    .. dto.Details.Where(detail =>
                        !excludedSeverityIds.Contains(detail.SeverityId))
                ];

                return dto with { Details = details };
            })
            .Where(dto => !hasExcludedSeverities || dto.Details.Count > 0);
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
        IReadOnlyList<InfraAssessmentReport> reports =
            await resourceProvider.GetResourceSummaries(ctx);

        return reports
            .Where(report =>
                string.IsNullOrEmpty(namespaceName) ||
                report.Resource.NamespaceName.Value == namespaceName)
            .SelectMany(static report => report.ToDenormalizedDtos());
    }
}
