using TrivyOperator.Dashboard.Application.Shared.EventPipeline.EventPipelineStarters.Abstractions;

namespace TrivyOperator.Dashboard.Application.Shared.EventPipeline.HostedServices;

public sealed class EventPipelineHostedService(
    IEnumerable<IEventPipelineStarter> services,
    IEnumerable<IWatchersStarter> watchersStarters,
    ILogger<EventPipelineHostedService> logger
) : BackgroundService
{
    public override async Task StopAsync(CancellationToken ctx)
    {
        logger.LogInformation("Event Pipeline Hosted Service is stopping.");
        await base.StopAsync(ctx);
        logger.LogInformation("Event Pipeline Hosted Service stopped.");
    }

    protected override async Task ExecuteAsync(CancellationToken ctx)
    {
        logger.LogInformation("Event Pipeline Hosted Service started.");

        foreach (IEventPipelineStarter service in services)
        {
            await service.StartPipeline(ctx);
        }

        // watchers that publish into several pipelines start once all pipelines are ready
        foreach (IWatchersStarter watchersStarter in watchersStarters)
        {
            await watchersStarter.StartWatchers(ctx);
        }
    }
}
