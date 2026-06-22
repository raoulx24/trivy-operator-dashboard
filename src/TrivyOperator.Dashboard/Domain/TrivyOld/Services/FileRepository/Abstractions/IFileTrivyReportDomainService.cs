using k8s;
using k8s.Models;

namespace TrivyOperator.Dashboard.Domain.Trivy.Services.FileRepository.Abstractions;

public interface IFileTrivyReportDomainService<TTrivyReport>
    where TTrivyReport : IKubernetesObject<V1ObjectMeta>, IMetadata<V1ObjectMeta>
{
    Task<IList<TTrivyReport>> GetAllReportsAsync(CancellationToken? cancellationToken = null);
    Task<IList<TTrivyReport>> GetAllReportsAsync(string key, CancellationToken? cancellationToken = null);
}
