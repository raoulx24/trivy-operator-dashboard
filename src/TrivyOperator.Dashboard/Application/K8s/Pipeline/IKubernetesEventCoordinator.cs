using TrivyOperator.Dashboard.Infrastructure.Utils;

namespace TrivyOperator.Dashboard.Application.K8s.Pipeline;

public interface IKubernetesEventCoordinator
{
    Task Start(CancellationToken cancellationToken, string watcherKey = CacheUtils.DefaultCacheRefreshKey);
}
