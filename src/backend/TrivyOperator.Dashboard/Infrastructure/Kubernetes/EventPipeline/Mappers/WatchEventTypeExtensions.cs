using k8s;
using TrivyOperator.Dashboard.Application.Shared.Models;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Mappers;

public static class WatchEventTypeExtensions
{
    public static PipelineEventType ToWatcherEvent(this WatchEventType watchEvent) => watchEvent switch
    {
        WatchEventType.Added => PipelineEventType.Added,
        WatchEventType.Modified => PipelineEventType.Modified,
        WatchEventType.Deleted => PipelineEventType.Deleted,
        WatchEventType.Error => PipelineEventType.Error,
        WatchEventType.Bookmark => PipelineEventType.Bookmark,
        _ => PipelineEventType.Unknown, // Handle Bookmark or any unexpected values
    };
}
