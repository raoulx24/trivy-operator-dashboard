using k8s;
using TrivyOperator.Dashboard.Application.Kubernetes.Models;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Mappers;

public static class WatchEventTypeExtensions
{
    public static WatcherEventType ToWatcherEvent(this WatchEventType watchEvent) => watchEvent switch
    {
        WatchEventType.Added => WatcherEventType.Added,
        WatchEventType.Modified => WatcherEventType.Modified,
        WatchEventType.Deleted => WatcherEventType.Deleted,
        WatchEventType.Error => WatcherEventType.Error,
        WatchEventType.Bookmark => WatcherEventType.Bookmark,
        _ => WatcherEventType.Unknown, // Handle Bookmark or any unexpected values
    };
}
