using TrivyOperator.Dashboard.Application.CveEnrichment.Models;
using TrivyOperator.Dashboard.Domain.CveEnrichment.Entities;

namespace TrivyOperator.Dashboard.Application.CveEnrichment.Abstractions;

public interface IEuvdProvider
{
    // all EUVD exploited vulnerabilities, downloaded page by page; one entry per CVE alias
    // a failure on any page fails the whole download, so a partial catalog is never returned
    Task<CveProviderResult<EuvdEntry>> GetExploited(string baseUrl, int pageSize, CancellationToken ctx = default);
}
