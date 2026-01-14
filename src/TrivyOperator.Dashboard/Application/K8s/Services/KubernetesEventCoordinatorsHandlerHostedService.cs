using TrivyOperator.Dashboard.Application.K8s.Services.EventCoordinators.Abstractions;

namespace TrivyOperator.Dashboard.Application.K8s.Services;

public sealed class KubernetesEventCoordinatorsHandlerHostedService(
    IEnumerable<IClusterScopedKubernetesEventCoordinator> services,
    ILogger<KubernetesEventCoordinatorsHandlerHostedService> logger
) : BackgroundService
{
    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Kubernetes Watcher Hosted Service is stopping.");
        await base.StopAsync(stoppingToken);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Kubernetes Watcher Hosted Service started.");
        foreach (IClusterScopedKubernetesEventCoordinator service in services)
        {
            await service.Start(cancellationToken);
        }
    }
}
