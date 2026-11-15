using TrivyOperator.Dashboard.Application.CveDetails.Queries.Models;
using TrivyOperator.Dashboard.Application.CveDetails.Queries.Services.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.Queries;
using TrivyOperator.Dashboard.Application.Shared.Queries.Services;

namespace TrivyOperator.Dashboard.Application.CveDetails.Queries.Services;

public class CveDetailsNullService : QueryNullService, ICveDetailsService
{
    public Task<CveDetailsResult> GetById(string cveId, CancellationToken ctx = default) =>
        Task.FromResult(CveDetailsResult.Of(CveDetailsLookupOutcomes.Disabled));
}
