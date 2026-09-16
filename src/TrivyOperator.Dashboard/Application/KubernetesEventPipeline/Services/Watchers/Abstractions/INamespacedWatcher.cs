using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

namespace TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.Watchers.Abstractions;

public interface INamespacedWatcher : IKubernetesWatcher
{
    Task ReconcileNamespaces(ContextName contextName, IReadOnlyList<NamespaceName> newNamespaceNames, CancellationToken ctx = default);
}
