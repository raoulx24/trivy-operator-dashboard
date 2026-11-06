using TrivyOperator.Dashboard.Application.CveEnrichment.Queries.Models;
using TrivyOperator.Dashboard.Application.CveEnrichment.Queries.Services.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.Queries;
using TrivyOperator.Dashboard.Application.Shared.Queries.Services;

namespace TrivyOperator.Dashboard.Application.CveEnrichment.Queries.Services;

public class CveEnrichmentNullService : QueryNullService, ICveEnrichmentService
{
    public Task<IReadOnlyList<CveInfoDto>> GetAll(CancellationToken ctx = default) =>
        Task.FromResult<IReadOnlyList<CveInfoDto>>([]);

    public Task<CveInfoDto?> GetById(string cveId, CancellationToken ctx = default) =>
        Task.FromResult<CveInfoDto?>(null);

    public void RequestRebuild() { }
}
