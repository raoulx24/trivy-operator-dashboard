using TrivyOperator.Dashboard.Application.Shared.EventPipeline.EventPipelineStarters.Abstractions;

namespace TrivyOperator.Dashboard.Application.Shared.EventPipeline.HostedServices;

public sealed class EventPipelineHostedService(
    IEnumerable<IEventPipelineStarter> services,
    ILogger<EventPipelineHostedService> logger
) : BackgroundService
{
    public override async Task StopAsync(CancellationToken ctx)
    {
        logger.LogInformation("Event Pipeline Hosted Service is stopping.");
        await base.StopAsync(ctx);
        logger.LogInformation("Event Pipeline Hosted Service stopped.");
    }

    protected override Task ExecuteAsync(CancellationToken ctx)
    {
        logger.LogInformation("Event Pipeline Hosted Service started.");

        foreach (IEventPipelineStarter service in services)
        {
            service.StartPipeline(ctx);
        }

        return Task.CompletedTask;
    }
}
