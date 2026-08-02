using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;

namespace TrivyOperator.Dashboard.Infrastructure.FileRepository.Services.Abstractions;

public interface IFileTrivyReportService<TReport, TKey>
where TReport : ITrivyReport<TKey>
{
    Task<IReadOnlyDictionary<TKey, TReport>> GetReportsAsync(CancellationToken ctx = default);
}
