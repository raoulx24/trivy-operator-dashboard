using TrivyOperator.Dashboard.Application.CveDetails.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.CveSubjects.Abstractions;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

namespace TrivyOperator.Dashboard.Application.CveDetails.Services;

// default context: the vulnerability reports are watched, their CVEs are the CVE subjects (a single context)
public sealed class CveSubjectsCveDetailsSubjects(ICveSubjects subjects) : ICveDetailsSubjects
{
    public Task<bool> Contains(CveId id, CancellationToken ctx = default) => Task.FromResult(subjects.Contains(id));
}
