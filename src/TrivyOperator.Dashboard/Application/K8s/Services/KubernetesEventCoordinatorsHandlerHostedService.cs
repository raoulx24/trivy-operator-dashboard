using TrivyOperator.Dashboard.Application.K8s.Models.WatcherEvents;
using TrivyOperator.Dashboard.Application.K8s.Services.EventCoordinators.Abstractions;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.K8s.ClientFactory.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.K8s.Contexts.Abstractions;

namespace TrivyOperator.Dashboard.Application.K8s.Services;

public sealed class KubernetesEventCoordinatorsHandlerHostedService(
    IEnumerable<IClusterScopedKubernetesEventCoordinator> services,
    IKubernetesContextAccessor contextAccessor,
    IKubernetesClientFactory clientFactory,
    ILogger<KubernetesEventCoordinatorsHandlerHostedService> logger
) : BackgroundService
{
    public override async Task StopAsync(CancellationToken ctx)
    {
        logger.LogInformation("Kubernetes Watcher Hosted Service is stopping.");
        await base.StopAsync(ctx);
        logger.LogInformation("Kubernetes Watcher Hosted Service stopped.");
    }

    protected override async Task ExecuteAsync(CancellationToken ctx)
    {
        logger.LogInformation("Kubernetes Watcher Hosted Service started.");
        ContextName contextName = clientFactory.GetDefaultContext();
        NamespaceName namespaceName = new();
        WatcherKey watcherKey = new(contextName, namespaceName);
        
        contextAccessor.Push(new ContextName());
        
        foreach (IClusterScopedKubernetesEventCoordinator service in services)
        {
            await service.Start(watcherKey, ctx);
        }
    }
}
