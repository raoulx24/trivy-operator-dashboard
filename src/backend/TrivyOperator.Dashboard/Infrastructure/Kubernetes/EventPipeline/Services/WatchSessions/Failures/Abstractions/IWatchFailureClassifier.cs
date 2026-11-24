using TrivyOperator.Dashboard.Application.Kubernetes.Models;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.WatchSessions.Failures.Abstractions;

public interface IWatchFailureClassifier
{
    // maps an exception raised in a phase of the watch session to a category, HTTP status and Retry-After
    // our own cancellation must be handled by the caller, before classifying
    WatchFailure Classify(Exception exception, WatchPhase phase, int consecutiveFailures, DateTime failingSince);
}
