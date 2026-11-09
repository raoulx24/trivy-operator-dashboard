using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Vulnerabilities;

namespace TrivyOperator.Dashboard.Domain.History.FixedVersionHistory.Entities;

// what the entries of a (vulnerability id, package) say over time
// FirstAt is the baseline's moment (the pair was first seen); FixSince is the moment since when a fix exists without
// interruption (a change from one fixed version to another keeps it), null when there is no fix now.
// Both are lower bounds: nothing is older than the moment the pair was first recorded.
public sealed record FixedVersionTimeline(
    PackageVulnerabilityKey Key,
    FixedVersionEntry Latest,
    Timestamp FirstAt,
    Timestamp? FixSince)
{
    // the moment the current fix appeared after the pair was first seen; null when there is no fix now or when the
    // fix was already there at the baseline (then it did not "appear", it came with the pair)
    public Timestamp? FixAppearedAt => FixSince is { } fixSince && fixSince > FirstAt ? fixSince : null;

    // entries oldest first; null when there are none
    public static FixedVersionTimeline? FromEntries(PackageVulnerabilityKey key, IReadOnlyList<FixedVersionEntry> entries)
    {
        if (entries.Count == 0)
        {
            return null;
        }

        Timestamp? fixSince = null;

        foreach (FixedVersionEntry entry in entries)
        {
            if (!entry.HasFix)
            {
                fixSince = null;
            }
            else if (fixSince is null)
            {
                fixSince = entry.At;
            }
        }

        return new FixedVersionTimeline(key, entries[^1], entries[0].At, fixSince);
    }
}
