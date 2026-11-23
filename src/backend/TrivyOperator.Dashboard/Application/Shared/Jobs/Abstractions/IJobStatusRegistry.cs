using TrivyOperator.Dashboard.Application.Shared.Jobs.Models;

namespace TrivyOperator.Dashboard.Application.Shared.Jobs.Abstractions;

// the status of the background (timed) jobs: each job pings it when it starts, finishes, fails or is skipped; the home
// page (Data health) reads it
// - it only reports; it never throws and never changes what a job does
// - the calls of a job that was not registered register it on the fly (group = name, no interval)
public interface IJobStatusRegistry
{
    // at the start of the hosted service, so the job shows up (NotRun) before its first run; again with the same name keeps
    // what it already reported
    void Register(JobDefinition definition);

    void Started(string name);

    // nextRunAt: when the job runs again if that is not simply its interval after now
    void Succeeded(string name, string? summary = null, DateTime? nextRunAt = null);

    void Failed(string name, string error, DateTime? nextRunAt = null);

    void Skipped(string name, string reason);

    IReadOnlyList<JobStatus> GetAll();
}
