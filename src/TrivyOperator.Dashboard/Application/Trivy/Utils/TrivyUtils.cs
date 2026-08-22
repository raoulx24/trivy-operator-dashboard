using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Application.Trivy.Utils;

public static class TrivyUtils
{
    public static List<int>? GetExcludedSeverityIdsFromStringList(string? excludedSeverities)
    {
        List<int> excludedSeverityIds = [];
        List<int> knownSeverityIds = [.. Severity.RankedSeverities.Select(x => x.Rank),];
        if (string.IsNullOrWhiteSpace(excludedSeverities))
        {
            return excludedSeverityIds;
        }

        string[] excludedStringSeverities = excludedSeverities.Split(',');
        foreach (string excludedSeverity in excludedStringSeverities)
        {
            if (int.TryParse(excludedSeverity, out int vulnerabilityId))
            {
                if (!knownSeverityIds.Contains(vulnerabilityId))
                {
                    return null;
                }

                excludedSeverityIds.Add(vulnerabilityId);
            }
            else
            {
                return null;
            }
        }

        return excludedSeverityIds;
    }
}
