using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Application.Trivy.Utils;

public static class TrivyUtils
{
    public static IReadOnlySet<int>? GetSeverityIdsToInclude(
        string? excludedSeverities)
    {
        HashSet<int> knownSeverityIds = [.. GetAllSeverityIds(),];

        if (string.IsNullOrWhiteSpace(excludedSeverities))
        {
            return knownSeverityIds;
        }

        foreach (string value in excludedSeverities.Split(','))
        {
            if (!int.TryParse(value, out int severityId) ||
                !knownSeverityIds.Contains(severityId))
            {
                return null;
            }

            knownSeverityIds.Remove(severityId);
        }

        return knownSeverityIds;
    }

    public static IReadOnlySet<int> GetAllSeverityIds() =>
        Severity.RankedSeverities.Select(static x => x.Rank).ToHashSet();

    // public static IReadOnlySet<int>? GetExcludedSeverityIdsFromStringList(string? excludedSeverities)
    // {
    //     if (string.IsNullOrWhiteSpace(excludedSeverities))
    //     {
    //         return new HashSet<int>();
    //     }
    //
    //     HashSet<int> knownSeverityIds = [.. Severity.RankedSeverities.Select(static x => x.Rank),];
    //
    //     HashSet<int> excludedSeverityIds = [];
    //
    //     foreach (string value in excludedSeverities.Split(','))
    //     {
    //         if (!int.TryParse(value, out int severityId) ||
    //             !knownSeverityIds.Contains(severityId))
    //         {
    //             return null;
    //         }
    //
    //         excludedSeverityIds.Add(severityId);
    //     }
    //
    //     return excludedSeverityIds;
    // }
}
