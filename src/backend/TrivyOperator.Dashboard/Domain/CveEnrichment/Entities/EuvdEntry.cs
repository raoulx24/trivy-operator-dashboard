using TrivyOperator.Dashboard.Domain.CveEnrichment.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

namespace TrivyOperator.Dashboard.Domain.CveEnrichment.Entities;

// ENISA EU Vulnerability Database entry from its exploited vulnerabilities list
// Epss is normalized to 0..1 (EUVD returns it as 0..100), to be comparable with EpssScore.Score
public sealed record EuvdEntry(
    CveId Id,
    EuvdId EuvdId,
    Timestamp? ExploitedSince,
    double? Epss);
