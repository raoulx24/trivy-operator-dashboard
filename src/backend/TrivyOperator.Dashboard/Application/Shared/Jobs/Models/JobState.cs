namespace TrivyOperator.Dashboard.Application.Shared.Jobs.Models;

// where a background job is, as it last reported
public enum JobState
{
    // registered, but no run yet (a job that waits a full interval before its first run stays here until then)
    NotRun,
    Running,
    Ok,
    Failed,
    // switched off by its options at the time of the run (e.g. GraceInHours 0); not a failure
    Skipped,
}
