using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Domain.TrivyOld.Services.FileRepositoryOld.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemoryOld.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Caching.InMemoryOld;

public class FileResourcePassthroughCache<TTrivyReport>(IFileTrivyReportDomainService<TTrivyReport> domain)
    : ResourcePassthroughCache<TTrivyReport>
    where TTrivyReport : IKubernetesObject<V1ObjectMeta>, IMetadata<V1ObjectMeta>
{
    protected override Task<IList<TTrivyReport>> FetchAllAsync(CancellationToken cancellationToken = default) =>
        domain.GetAllReportsAsync(cancellationToken);

    protected override Task<IList<TTrivyReport>> FetchByKeyAsync(
        string key,
        CancellationToken cancellationToken = default
    ) => domain.GetAllReportsAsync(key, cancellationToken);
}
