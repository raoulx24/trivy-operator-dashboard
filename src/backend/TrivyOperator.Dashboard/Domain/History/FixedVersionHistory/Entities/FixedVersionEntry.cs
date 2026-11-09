using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Vulnerabilities;

namespace TrivyOperator.Dashboard.Domain.History.FixedVersionHistory.Entities;

// one value of the fixed version of a (vulnerability id, package), as seen in the report modified at At
// (the report's updateTimestamp). FixedVersion is null when there is no fix.
public sealed record FixedVersionEntry
{
    public PackageVersion? FixedVersion { get; }
    public Timestamp At { get; }

    public FixedVersionEntry(PackageVersion? fixedVersion, Timestamp at)
    {
        // "no fix" has one representation: null (an empty or N/A version is not a fix)
        FixedVersion = fixedVersion is { IsValid: true } ? fixedVersion : null;
        At = at;
    }

    public bool HasFix => FixedVersion is not null;

    public bool HasSameFixedVersion(FixedVersionEntry other) =>
        string.Equals(FixedVersion?.Value, other.FixedVersion?.Value, StringComparison.Ordinal);
}
