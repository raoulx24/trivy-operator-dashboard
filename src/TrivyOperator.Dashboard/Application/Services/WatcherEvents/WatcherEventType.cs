using k8s;

namespace TrivyOperator.Dashboard.Application.Services.WatcherEvents;

public enum WatcherEventType
{
    Initialized,
    InitialAdded,
    Added,
    Modified,
    Deleted,
    Error,
    Bookmark,
    WatcherConnected,
    Flushed,
    Unknown,
}

public static class WatchEventTypeExtensions
{
    public static WatcherEventType ToWatcherEvent(this WatchEventType watchEvent)
    {
        return watchEvent switch
        {
            WatchEventType.Added => WatcherEventType.Added,
            WatchEventType.Modified => WatcherEventType.Modified,
            WatchEventType.Deleted => WatcherEventType.Deleted,
            WatchEventType.Error => WatcherEventType.Error,
            WatchEventType.Bookmark => WatcherEventType.Bookmark,
            _ => WatcherEventType.Unknown, // Handle Bookmark or any unexpected values
        };
    }
}
