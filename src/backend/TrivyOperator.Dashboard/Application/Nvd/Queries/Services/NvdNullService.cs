using TrivyOperator.Dashboard.Application.Nvd.Queries.Models;
using TrivyOperator.Dashboard.Application.Nvd.Queries.Services.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.Queries;
using TrivyOperator.Dashboard.Application.Shared.Queries.Services;

namespace TrivyOperator.Dashboard.Application.Nvd.Queries.Services;

public class NvdNullService : QueryNullService, INvdService
{
    public Task<NvdCveResult> GetById(string cveId, CancellationToken ctx = default) =>
        Task.FromResult(NvdCveResult.Of(NvdLookupOutcomes.Disabled));
}
