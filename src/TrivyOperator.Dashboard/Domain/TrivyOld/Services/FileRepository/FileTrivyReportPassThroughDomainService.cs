using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.TrivyOld.Services.FileRepository.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.K8s.Services.Abstractions;

namespace TrivyOperator.Dashboard.Domain.TrivyOld.Services.FileRepository;

public class FileTrivyReportPassThroughService<TTrivyReport, TTrivyReportList>(
    IFileTrivyReportDomainService<TTrivyReport> fileTrivyReportDomainService
) : INamespacedResourceWatchService<TTrivyReport, TTrivyReportList>
    where TTrivyReport : IKubernetesObject<V1ObjectMeta>, IMetadata<V1ObjectMeta>
    where TTrivyReportList : IKubernetesObject<V1ListMeta>, IItems<TTrivyReport>
{
    public async Task<TTrivyReport> GetResource(
        string resourceName,
        string namespaceName,
        CancellationToken? cancellationToken = null
    )
    {
        IList<TTrivyReport> resources =
            await fileTrivyReportDomainService.GetAllReportsAsync(namespaceName, cancellationToken);
        return resources.First(r => r.Metadata.Name == resourceName);
    }

    public Task<TTrivyReportList> GetResourceList(
        string namespaceName,
        int? pageLimit = null,
        string? continueToken = null,
        CancellationToken? cancellationToken = null
    ) => throw new NotImplementedException();

    public Task<IList<TTrivyReport>> GetResources(CancellationToken? cancellationToken = null) =>
        fileTrivyReportDomainService.GetAllReportsAsync(cancellationToken);

    public Task<IList<TTrivyReport>> GetResources(string namespaceName, CancellationToken? cancellationToken = null) =>
        fileTrivyReportDomainService.GetAllReportsAsync(namespaceName, cancellationToken);

    public IAsyncEnumerable<WatchEvent<TTrivyReport>> GetResourceWatchList(
        string namespaceName,
        string? lastResourceVersion = null,
        int? timeoutSeconds = null,
        Action<Exception>? onError = null,
        CancellationToken? cancellationToken = null
    ) => throw new NotImplementedException();
}
