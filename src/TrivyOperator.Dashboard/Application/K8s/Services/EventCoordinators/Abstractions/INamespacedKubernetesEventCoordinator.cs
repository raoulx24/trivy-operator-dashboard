using TrivyOperator.Dashboard.Application.K8s.Models.WatcherEvents;
using TrivyOperator.Dashboard.Application.K8s.Pipeline;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

namespace TrivyOperator.Dashboard.Application.K8s.Services.EventCoordinators.Abstractions;

public interface INamespacedKubernetesEventCoordinator : IKubernetesEventCoordinator
{
    Task Stop(WatcherKey key, CancellationToken ctx = default);
    Task ReconcileWatchers(ContextName contextName, IReadOnlyList<NamespaceName> newNamespaceNames, CancellationToken ctx = default);
}
