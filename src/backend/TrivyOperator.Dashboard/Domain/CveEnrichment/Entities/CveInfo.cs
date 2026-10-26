using TrivyOperator.Dashboard.Domain.CveEnrichment.ValueObjects;

namespace TrivyOperator.Dashboard.Domain.CveEnrichment.Entities;

// the enrichment of one CVE; each source writes only its own part
// a null part means the source did not process the CVE yet (or the source is disabled)
public sealed record CveInfo(
    CveId Id,
    CveSourceResult<KevEntry>? Kev,
    CveSourceResult<EpssScore>? Epss,
    CveSourceResult<EuvdEntry>? Euvd)
{
    public static CveInfo Empty(CveId id) => new(id, null, null, null);
}
