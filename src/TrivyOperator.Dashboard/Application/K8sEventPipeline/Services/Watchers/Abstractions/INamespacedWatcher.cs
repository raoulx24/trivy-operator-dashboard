using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

namespace TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.Watchers.Abstractions;

public interface INamespacedWatcher : IKubernetesWatcher
{
    Task ReconcileNamespaces(ContextName contextName, IReadOnlyList<NamespaceName> newNamespaceNames, CancellationToken ctx = default);
}
