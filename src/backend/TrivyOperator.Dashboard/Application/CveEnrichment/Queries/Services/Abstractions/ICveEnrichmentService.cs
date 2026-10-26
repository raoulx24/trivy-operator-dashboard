using TrivyOperator.Dashboard.Application.CveEnrichment.Queries.Models;

namespace TrivyOperator.Dashboard.Application.CveEnrichment.Queries.Services.Abstractions;

public interface ICveEnrichmentService
{
    Task<IReadOnlyList<CveInfoDto>> GetAll(CancellationToken ctx = default);

    // null when the id is not a CVE or the CVE is not seen in any report
    Task<CveInfoDto?> GetById(string cveId, CancellationToken ctx = default);

    // marks all sources for refetch and wakes up the processing
    void RequestRebuild();
}
