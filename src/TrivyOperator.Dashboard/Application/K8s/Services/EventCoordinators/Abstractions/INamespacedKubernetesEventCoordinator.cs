using TrivyOperator.Dashboard.Infrastructure.Utils;

namespace TrivyOperator.Dashboard.Application.K8s.Services.EventCoordinators.Abstractions;

public interface INamespacedKubernetesEventCoordinator : IKubernetesEventCoordinator
{
    Task Stop(CancellationToken cancellationToken, string watcherKey = CacheUtils.DefaultCacheRefreshKey);
    Task ReconcileWatchers(string[] newNamespaceNames, CancellationToken cancellationToken);
}
