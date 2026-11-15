using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Infrastructure.Shared.Utils;

// the CVSS qualitative severity, for the CVE details providers
public static class CvssSeverities
{
    // Severity throws on unknown values; the CVSS names are a known set, anything else is Unknown
    public static Severity FromName(string? value) =>
        value?.Trim().ToUpperInvariant() switch
        {
            "CRITICAL" => Severities.Critical,
            "HIGH" => Severities.High,
            "MEDIUM" => Severities.Medium,
            "LOW" => Severities.Low,
            "NONE" => Severities.None,
            _ => Severities.Unknown,
        };

    // for a score without a name (CVSS v2 in the CVE.org records) - the v3.x/v4.0 ranges, with 0.0 as None; the v2
    // ranges (Low 0-3.9, Medium 4-6.9, High 7-10) have no Critical, so a v2 score of 9+ is High
    public static Severity FromScore(decimal score, bool isV2 = false) =>
        score switch
        {
            <= 0m => Severities.None,
            < 4m => Severities.Low,
            < 7m => Severities.Medium,
            < 9m => Severities.High,
            _ => isV2 ? Severities.High : Severities.Critical,
        };
}
