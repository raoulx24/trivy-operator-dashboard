using TrivyOperator.Dashboard.Application.CveEnrichment.Models;
using TrivyOperator.Dashboard.Domain.CveEnrichment.Entities;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

namespace TrivyOperator.Dashboard.Application.CveEnrichment.Abstractions;

public interface IEpssProvider
{
    // one batch; the caller keeps the batch within the configured size
    // CVEs without a score are missing from the result
    Task<CveProviderResult<EpssScore>> GetScores(
        string baseUrl,
        IReadOnlyCollection<CveId> ids,
        CancellationToken ctx = default);
}
