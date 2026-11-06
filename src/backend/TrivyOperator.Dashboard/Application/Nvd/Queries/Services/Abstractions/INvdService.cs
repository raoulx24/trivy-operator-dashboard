using TrivyOperator.Dashboard.Application.Nvd.Queries.Models;

using TrivyOperator.Dashboard.Application.Shared.Queries;
using TrivyOperator.Dashboard.Application.Shared.Queries.Services.Abstractions;

namespace TrivyOperator.Dashboard.Application.Nvd.Queries.Services.Abstractions;

public interface INvdService : IQueryService
{
    // see NvdLookupOutcome; the dto is set only when Found
    Task<NvdCveResult> GetById(string cveId, CancellationToken ctx = default);
}
