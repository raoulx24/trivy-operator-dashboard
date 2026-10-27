using TrivyOperator.Dashboard.Application.Nvd.Queries.Models;

namespace TrivyOperator.Dashboard.Application.Nvd.Queries.Services.Abstractions;

public interface INvdService
{
    // see NvdLookupOutcome; the dto is set only when Found
    Task<NvdCveResult> GetById(string cveId, CancellationToken ctx = default);
}
