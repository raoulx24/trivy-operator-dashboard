using k8s;
using k8s.Models;

namespace TrivyOperator.Dashboard.Application.K8s.Services.Watchers.Abstractions;

public interface INamespacedWatcher<TKubernetesObject> : IKubernetesWatcher<TKubernetesObject>
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>
{
    Task Delete(string watcherKey, CancellationToken cancellationToken);
    Task ReconcileNamespaces(string[] newNamespaceNames, CancellationToken cancellationToken);
}
