using TrivyOperator.Dashboard.Application.CveDetails.Queries.Models;

using TrivyOperator.Dashboard.Application.Shared.Queries;
using TrivyOperator.Dashboard.Application.Shared.Queries.Services.Abstractions;

namespace TrivyOperator.Dashboard.Application.CveDetails.Queries.Services.Abstractions;

public interface ICveDetailsService : IQueryService
{
    // see CveDetailsLookupOutcome; the dto is set only when Found
    Task<CveDetailsResult> GetById(string cveId, CancellationToken ctx = default);
}
