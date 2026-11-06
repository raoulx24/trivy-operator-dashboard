using TrivyOperator.Dashboard.Application.CveEnrichment.Queries.Models;

using TrivyOperator.Dashboard.Application.Shared.Queries;
using TrivyOperator.Dashboard.Application.Shared.Queries.Services.Abstractions;

namespace TrivyOperator.Dashboard.Application.CveEnrichment.Queries.Services.Abstractions;

public interface ICveEnrichmentService : IQueryService
{
    Task<IReadOnlyList<CveInfoDto>> GetAll(CancellationToken ctx = default);

    // null when the id is not a CVE or the CVE is not seen in any report
    Task<CveInfoDto?> GetById(string cveId, CancellationToken ctx = default);

    // marks all sources for refetch and wakes up the processing
    void RequestRebuild();
}
