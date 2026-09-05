using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.EventPipelineStarters.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.K8s.ClientFactory.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.K8s.Contexts.Abstractions;

namespace TrivyOperator.Dashboard.Application.K8sEventPipeline.HostedServices;

public sealed class KubernetesEventPipelineHost(
    IEnumerable<IKubernetesEventPipelineStarter> services,
    IKubernetesContextResolver contextResolver,
    IKubernetesClientFactory clientFactory,
    ILogger<KubernetesEventPipelineHost> logger
) : BackgroundService
{
    public override async Task StopAsync(CancellationToken ctx)
    {
        logger.LogInformation("Kubernetes Watcher Hosted Service is stopping.");
        await base.StopAsync(ctx);
        logger.LogInformation("Kubernetes Watcher Hosted Service stopped.");
    }

    protected override Task ExecuteAsync(CancellationToken ctx)
    {
        logger.LogInformation("Kubernetes Watcher Hosted Service started.");

        foreach (IKubernetesEventPipelineStarter service in services)
        {
            service.StartPipeline(ctx);
        }

        return Task.CompletedTask;
    }
}
