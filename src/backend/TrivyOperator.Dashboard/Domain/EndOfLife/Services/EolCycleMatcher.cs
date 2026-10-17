using TrivyOperator.Dashboard.Domain.EndOfLife.Entities;

namespace TrivyOperator.Dashboard.Domain.EndOfLife.Services;

// matches a version against release cycles by longest prefix ending on a boundary
// e.g. 3.18.4 -> 3.18, 12.5 -> 12, 22.04 -> 22.04, 2 (Karoo) -> 2; but 2023 does not match cycle 2
public static class EolCycleMatcher
{
    public static EolRelease? Match(IReadOnlyList<EolRelease> releases, string version)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            return null;
        }

        string trimmedVersion = version.Trim();
        EolRelease? bestMatch = null;
        int bestLength = 0;

        foreach (EolRelease release in releases)
        {
            if (!release.Cycle.IsValid)
            {
                continue;
            }

            string cycle = release.Cycle.Value;

            if (cycle.Length <= bestLength || !IsPrefixOnBoundary(trimmedVersion, cycle))
            {
                continue;
            }

            bestMatch = release;
            bestLength = cycle.Length;
        }

        return bestMatch;
    }

    private static bool IsPrefixOnBoundary(string version, string cycle) =>
        version.StartsWith(cycle, StringComparison.OrdinalIgnoreCase) &&
        (version.Length == cycle.Length || !char.IsLetterOrDigit(version[cycle.Length]));
}
