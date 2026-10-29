using TrivyOperator.Dashboard.Application.CveEnrichment.Queries.Models;

namespace TrivyOperator.Dashboard.Application.CveEnrichment.Queries.Services.Abstractions;

public interface ICveEnrichmentService
{
    // false for the null service - CVE Enrichment disabled, or no vulnerability reports (see CveEnrichmentCompositionResolver)
    bool IsEnabled { get; }

    Task<IReadOnlyList<CveInfoDto>> GetAll(CancellationToken ctx = default);

    // null when the id is not a CVE or the CVE is not seen in any report
    Task<CveInfoDto?> GetById(string cveId, CancellationToken ctx = default);

    // marks all sources for refetch and wakes up the processing
    void RequestRebuild();
}
