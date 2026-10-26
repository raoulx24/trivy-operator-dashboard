using TrivyOperator.Dashboard.Domain.CveEnrichment.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

namespace TrivyOperator.Dashboard.Domain.CveEnrichment.Entities;

// CIRCL Vulnerability-Lookup data for one CVE - fetched on demand (per CVE, no cache), not by the background enrichment
// - sightings are community reported; only exploited and published-proof-of-concept are counted
//   ("seen" is mostly social media mentions, not evidence of exploitation)
// - KevStatusReasons - distinct status_reason values of the CVE entries across the KEV catalogs aggregated by CIRCL
public sealed record CirclInfo(
    CveId Id,
    int ExploitedSightings,
    int ProofOfConceptSightings,
    IReadOnlyList<CirclKevStatusReason> KevStatusReasons,
    Timestamp FetchedAt);
