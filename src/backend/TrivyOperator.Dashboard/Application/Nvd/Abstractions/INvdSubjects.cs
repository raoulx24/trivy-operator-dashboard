using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

namespace TrivyOperator.Dashboard.Application.Nvd.Abstractions;

// the CVEs NVD may be asked about: the ones of the vulnerability reports of the current context
// (the dashboard is not an open proxy to NVD, unless Nvd:AllowAnyCve)
public interface INvdSubjects
{
    Task<bool> Contains(CveId id, CancellationToken ctx = default);
}
