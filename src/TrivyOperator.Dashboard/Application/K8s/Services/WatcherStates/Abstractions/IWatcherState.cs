namespace TrivyOperator.Dashboard.Application.K8s.Services.WatcherStates.Abstractions;

public interface IWatcherState
{
    bool IsQueueProcessingStarted();
    void StartEventsProcessing(CancellationToken cancellationToken);
}