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
    Throttled,
    // an object that could not be deserialized was skipped; the watch goes on
    Skipped,
    Unknown,
}
