using TrivyOperator.Dashboard.Application.Nvd.Queries.Models;
using TrivyOperator.Dashboard.Application.Nvd.Queries.Services.Abstractions;

namespace TrivyOperator.Dashboard.Application.Nvd.Queries.Services;

public class NvdNullService : INvdService
{
    public Task<NvdCveDto?> GetById(string cveId, CancellationToken ctx = default) =>
        Task.FromResult<NvdCveDto?>(null);
}
