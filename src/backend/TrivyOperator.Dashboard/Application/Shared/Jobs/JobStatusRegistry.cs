using System.Collections.Concurrent;
using TrivyOperator.Dashboard.Application.Shared.Jobs.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.Jobs.Models;

namespace TrivyOperator.Dashboard.Application.Shared.Jobs;

public sealed class JobStatusRegistry : IJobStatusRegistry
{
    private readonly ConcurrentDictionary<string, JobStatus> statuses = new(StringComparer.Ordinal);

    public void Register(JobDefinition definition) =>
        statuses.AddOrUpdate(
            definition.Name,
            _ => New(definition.Name, definition.Group, definition.Interval),
            (_, current) => current with { Group = definition.Group, Interval = definition.Interval, });

    public void Started(string name) =>
        Update(name, current => current with { State = JobState.Running, LastStartedAt = DateTime.UtcNow, });

    public void Succeeded(string name, string? summary = null, DateTime? nextRunAt = null) =>
        Update(
            name,
            current =>
            {
                DateTime now = DateTime.UtcNow;

                return current with
                {
                    State = JobState.Ok,
                    LastFinishedAt = now,
                    LastSuccessAt = now,
                    NextRunAt = NextRun(current, now, nextRunAt),
                    Summary = summary,
                    LastError = null,
                    ConsecutiveFailures = 0,
                };
            });

    public void Failed(string name, string error, DateTime? nextRunAt = null) =>
        Update(
            name,
            current =>
            {
                DateTime now = DateTime.UtcNow;

                return current with
                {
                    State = JobState.Failed,
                    LastFinishedAt = now,
                    NextRunAt = NextRun(current, now, nextRunAt),
                    LastError = error,
                    ConsecutiveFailures = current.ConsecutiveFailures + 1,
                };
            });

    public void Skipped(string name, string reason) =>
        Update(
            name,
            current =>
            {
                DateTime now = DateTime.UtcNow;

                return current with
                {
                    State = JobState.Skipped,
                    LastFinishedAt = now,
                    NextRunAt = NextRun(current, now, null),
                    Summary = reason,
                };
            });

    public IReadOnlyList<JobStatus> GetAll() =>
        [.. statuses.Values.OrderBy(x => x.Group, StringComparer.Ordinal).ThenBy(x => x.Name, StringComparer.Ordinal),];

    private void Update(string name, Func<JobStatus, JobStatus> change) =>
        statuses.AddOrUpdate(name, key => change(New(key, key, TimeSpan.Zero)), (_, current) => change(current));

    private static JobStatus New(string name, string group, TimeSpan interval) =>
        new(name, group, interval, JobState.NotRun, null, null, null, null, null, null, 0);

    private static DateTime? NextRun(JobStatus current, DateTime now, DateTime? nextRunAt) =>
        nextRunAt ?? (current.Interval > TimeSpan.Zero ? now + current.Interval : null);
}
