using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Infrastructure.Utils;

namespace TrivyOperator.Dashboard.Application.K8s.Pipeline;

public interface IKubernetesWatcher
{
    Task Add(CancellationToken cancellationToken, string watcherKey = CacheUtils.DefaultCacheRefreshKey);
    Task Recreate(CancellationToken cancellationToken, string watcherKey = CacheUtils.DefaultCacheRefreshKey);
}

public interface IKubernetesWatcher<TKubernetesObject> : IKubernetesWatcher
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>;
