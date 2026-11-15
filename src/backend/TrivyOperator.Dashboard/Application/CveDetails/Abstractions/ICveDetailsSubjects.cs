using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

namespace TrivyOperator.Dashboard.Application.CveDetails.Abstractions;

// the CVEs the provider may be asked about: the ones of the vulnerability reports of the current context
// (the dashboard is not an open proxy to the provider, unless CveDetails:AllowAnyCve)
public interface ICveDetailsSubjects
{
    Task<bool> Contains(CveId id, CancellationToken ctx = default);
}
