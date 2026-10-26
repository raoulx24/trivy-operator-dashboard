using TrivyOperator.Dashboard.Domain.CveEnrichment.ValueObjects;

namespace TrivyOperator.Dashboard.Domain.CveEnrichment.Entities;

// FIRST Exploit Prediction Scoring System - probability (0..1) of exploitation activity in the next 30 days
// Percentile (0..1) is the rank among all scored CVEs; Date is the day the score was computed
public sealed record EpssScore(
    CveId Id,
    double Score,
    double Percentile,
    DateOnly Date);
