using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Application.Trivy.Utils;

public static class TrivyUtils
{
    public static IReadOnlySet<int>? GetIncludedSeverityIdsFromStringList(
        string? includedSeverities)
    {
        if (string.IsNullOrWhiteSpace(includedSeverities))
        {
            return [];
        }

        HashSet<int> knownSeverityIds = Severity.RankedSeverities
            .Select(static severity => severity.Rank)
            .ToHashSet();

        HashSet<int> includedSeverityIds = [];

        foreach (string value in includedSeverities.Split(','))
        {
            if (!int.TryParse(value.Trim(), out int severityId) ||
                !knownSeverityIds.Contains(severityId))
            {
                return null;
            }

            includedSeverityIds.Add(severityId);
        }

        return includedSeverityIds;
    }
    
    // TODO: old, i might not need it anymore
    public static IReadOnlySet<int>? GetExcludedSeverityIdsFromStringList(
        string? excludedSeverities
    )
    {
        if (string.IsNullOrWhiteSpace(excludedSeverities))
        {
            return new HashSet<int>();
        }

        HashSet<int> knownSeverityIds = Severity.RankedSeverities
            .Select(static x => x.Rank)
            .ToHashSet();

        HashSet<int> excludedSeverityIds = [];

        foreach (string value in excludedSeverities.Split(','))
        {
            if (!int.TryParse(value, out int severityId) ||
                !knownSeverityIds.Contains(severityId))
            {
                return null;
            }

            excludedSeverityIds.Add(severityId);
        }

        return excludedSeverityIds;
    }
}
