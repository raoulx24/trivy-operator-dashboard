using TrivyOperator.Dashboard.Application.Kubernetes.Contexts.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.Namespaces;
using TrivyOperator.Dashboard.Application.Kubernetes.Namespaces.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.WatcherRegistries.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.EventPipeline.EventPipelineStarters.Abstractions;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

namespace TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventPipelineStarters;

// starts the watchers of the namespaced report types, as the namespace policy says:
// - ClusterWide: one cluster wide scope per type (a missing RBAC shows as a Forbidden, Red watcher)
// - PerNamespace: one scope per type and watched namespace
// the cluster scoped report types start their own watcher with their pipeline
public sealed class NamespacedWatchersStarter(
    IEnumerable<INamespacedWatcherRegistry> namespacedWatcherRegistries,
    INamespacePolicyProvider namespacePolicyProvider,
    IKubernetesContextResolver contextResolver,
    ILogger<NamespacedWatchersStarter> logger
) : IWatchersStarter
{
    public async Task StartWatchers(CancellationToken ctx = default)
    {
        INamespacedWatcherRegistry[] registries = [.. namespacedWatcherRegistries,];

        if (registries.Length == 0)
        {
            return;
        }

        if (!contextResolver.TryGetCurrentContext(out ContextName contextName))
        {
            contextName = new ContextName();
        }

        NamespacePolicy namespacePolicy = namespacePolicyProvider.Get(contextName);

        WatcherScope[] scopes = namespacePolicy.Mode == WatchMode.ClusterWide
            ? [new WatcherScope(contextName, new NamespaceName()),]
            : [.. namespacePolicy.WatchedNamespaces.Select(namespaceName => new WatcherScope(contextName, namespaceName)),];

        logger.LogInformation(
            "Starting namespaced watchers - mode {mode} - {typeCount} types x {scopeCount} scopes",
            namespacePolicy.Mode,
            registries.Length,
            scopes.Length
        );

        Task[] tasks =
        [
            .. registries.SelectMany(registry => scopes.Select(scope => registry.StartWatcher(scope, ctx))),
        ];

        await Task.WhenAll(tasks);
    }
}
