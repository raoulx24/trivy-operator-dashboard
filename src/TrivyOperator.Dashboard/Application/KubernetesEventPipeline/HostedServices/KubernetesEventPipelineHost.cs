using TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.EventPipelineStarters.Abstractions;

namespace TrivyOperator.Dashboard.Application.KubernetesEventPipeline.HostedServices;

public sealed class KubernetesEventPipelineHost(
    IEnumerable<IKubernetesEventPipelineStarter> services,
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
