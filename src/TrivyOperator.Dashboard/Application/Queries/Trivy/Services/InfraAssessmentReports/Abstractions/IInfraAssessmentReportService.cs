using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Services.InfraAssessmentReports.Abstractions;

public interface IInfraAssessmentReportService
{
    Task<IEnumerable<InfraAssessmentReportDenormalizedDto>> GetInfraAssessmentReportDenormalizedDtos(
        string? namespaceName = null,
        CancellationToken ctx = default
    );

    Task<InfraAssessmentReportDto?> GetInfraAssessmentReportDtoByUid(string uid, CancellationToken ctx = default);

    Task<IEnumerable<InfraAssessmentReportDto>> GetInfraAssessmentReportDtos(
        string? namespaceName = null,
        IEnumerable<int>? excludedSeverities = null,
        CancellationToken ctx = default
    );
}
