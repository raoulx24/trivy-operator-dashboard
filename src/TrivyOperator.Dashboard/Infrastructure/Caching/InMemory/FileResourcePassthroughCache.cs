using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Domain.TrivyOld.Services.FileRepository.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Caching.InMemory;

public class FileResourcePassthroughCache<TTrivyReport>(IFileTrivyReportDomainService<TTrivyReport> domain)
    : ResourcePassthroughCache<TTrivyReport>
    where TTrivyReport : IKubernetesObject<V1ObjectMeta>, IMetadata<V1ObjectMeta>
{
    protected override Task<IList<TTrivyReport>> FetchAllAsync(CancellationToken? cancellationToken = null) =>
        domain.GetAllReportsAsync(cancellationToken);

    protected override Task<IList<TTrivyReport>> FetchByKeyAsync(
        string key,
        CancellationToken? cancellationToken = null
    ) => domain.GetAllReportsAsync(key, cancellationToken);
}
