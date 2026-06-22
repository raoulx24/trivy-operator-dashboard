namespace TrivyOperator.Dashboard.Domain.Trivy;

public static class TrivyUtils
{
    public static List<int>? GetExcludedSeverityIdsFromStringList(string? excludedSeverities)
    {
        List<int> excludedSeverityIds = [];
        List<int> knownSeverityIds = [.. (int[])Enum.GetValues(typeof(TrivySeverity)),];
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
