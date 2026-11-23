namespace TrivyOperator.Dashboard.Application.Shared.Jobs.Models;

// a snapshot of a background job, as it last reported (in memory: after a restart every job is NotRun again)
// - NextRunAt: when it runs again, as far as the job knows (the end of the last run plus its interval, or the retry time the
//   job gave, e.g. the Retry-After of a source)
// - Summary: what the last run did, as a short text ("12 removed", "1,234 CVEs"), or why it was skipped
// - LastError: the message of the last failed run; kept until the next run succeeds
public sealed record JobStatus(
    string Name,
    string Group,
    TimeSpan Interval,
    JobState State,
    DateTime? LastStartedAt,
    DateTime? LastFinishedAt,
    DateTime? LastSuccessAt,
    DateTime? NextRunAt,
    string? Summary,
    string? LastError,
    int ConsecutiveFailures);
