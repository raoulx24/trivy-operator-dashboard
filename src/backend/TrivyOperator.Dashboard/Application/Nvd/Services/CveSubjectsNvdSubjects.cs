using TrivyOperator.Dashboard.Application.Nvd.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.CveSubjects.Abstractions;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

namespace TrivyOperator.Dashboard.Application.Nvd.Services;

// default context: the vulnerability reports are watched, their CVEs are the CVE subjects (a single context)
public sealed class CveSubjectsNvdSubjects(ICveSubjects subjects) : INvdSubjects
{
    public Task<bool> Contains(CveId id, CancellationToken ctx = default) => Task.FromResult(subjects.Contains(id));
}
