using TrivyOperator.Dashboard.Application.CveEnrichment.Models;
using TrivyOperator.Dashboard.Domain.CveEnrichment.Entities;

namespace TrivyOperator.Dashboard.Application.CveEnrichment.Abstractions;

public interface IKevProvider
{
    // the whole CISA KEV catalog
    // validator - opaque value from a previous Ok result; when provided, the provider may answer with NotModified
    Task<CveProviderResult<KevEntry>> GetCatalog(string url, string? validator, CancellationToken ctx = default);
}
