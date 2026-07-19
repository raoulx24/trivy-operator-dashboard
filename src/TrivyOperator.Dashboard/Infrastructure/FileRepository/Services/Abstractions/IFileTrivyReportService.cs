using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;

namespace TrivyOperator.Dashboard.Infrastructure.FileRepository.Services.Abstractions;

public interface IFileTrivyReportService<TTrivyReport>
where TTrivyReport : ITrivyReport
{
    Task<IReadOnlyDictionary<NamespaceName, IReadOnlyCollection<TTrivyReport>>> GetReportsByNamespaceAsync(
        CancellationToken ctx = default
    );
}
