using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Application.K8s.Models.WatcherEvents;
using TrivyOperator.Dashboard.Application.K8s.Pipeline;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

namespace TrivyOperator.Dashboard.Application.K8s.Services.Watchers.Abstractions;

public interface INamespacedWatcher<TKubernetesObject> : IKubernetesWatcher<TKubernetesObject>
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>
{
    Task ReconcileNamespaces(ContextName contextName, NamespaceName[] newNamespaceNames, CancellationToken cancellationToken);
}
