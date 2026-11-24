using TrivyOperator.Dashboard.Application.Kubernetes.Models;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.WatchSessions.Failures.Abstractions;

public interface IWatchRetryPolicy
{
    // how long to wait before retrying after this failure
    TimeSpan GetDelay(WatchFailure failure);

    // whether the watcher should be reported as Red (failed) rather than Yellow (recovering)
    bool IsRed(WatchFailure? failure);
}
