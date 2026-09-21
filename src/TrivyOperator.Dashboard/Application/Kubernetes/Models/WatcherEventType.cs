namespace TrivyOperator.Dashboard.Application.Kubernetes.Models;

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
