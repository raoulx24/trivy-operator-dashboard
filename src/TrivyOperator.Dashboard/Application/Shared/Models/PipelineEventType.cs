namespace TrivyOperator.Dashboard.Application.Shared.Models;

public enum PipelineEventType
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
