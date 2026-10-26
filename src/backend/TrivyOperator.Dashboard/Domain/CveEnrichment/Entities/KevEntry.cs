using TrivyOperator.Dashboard.Domain.CveEnrichment.ValueObjects;

namespace TrivyOperator.Dashboard.Domain.CveEnrichment.Entities;

// CISA Known Exploited Vulnerabilities catalog entry - the CVE is confirmed as exploited in the wild
// KnownRansomwareCampaignUse - true for "Known", null for "Unknown" or missing
public sealed record KevEntry(
    CveId Id,
    DateOnly DateAdded,
    DateOnly DueDate,
    bool? KnownRansomwareCampaignUse,
    string RequiredAction);
