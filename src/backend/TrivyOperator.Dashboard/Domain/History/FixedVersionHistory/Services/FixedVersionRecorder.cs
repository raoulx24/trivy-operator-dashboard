using TrivyOperator.Dashboard.Domain.History.FixedVersionHistory.Entities;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Vulnerabilities;

namespace TrivyOperator.Dashboard.Domain.History.FixedVersionHistory.Services;

// decides what an incoming fixed version means for its pair (vulnerability id, package)
// - no state: the first entry, the baseline
// - only a report modified after the last entry can change the value: the reports of different images are
//   scanned at different times, with different Trivy DB versions, and an older one must not undo a newer one
//   (the arrival order still decides the baseline the very first time a pair is seen)
// - a different value from a newer report is a change; the same value is not recorded again
// - the pair is touched (for the housekeeping grace) when its touch is older than touchRefresh
public static class FixedVersionRecorder
{
    public static FixedVersionWrite? Decide(
        PackageVulnerabilityKey key,
        FixedVersionEntry incoming,
        FixedVersionState? state,
        Timestamp now,
        TimeSpan touchRefresh)
    {
        if (state is null)
        {
            return new FixedVersionWrite(FixedVersionWriteKind.Baseline, key, incoming, now);
        }

        if (incoming.At > state.Latest.At && !incoming.HasSameFixedVersion(state.Latest))
        {
            return new FixedVersionWrite(FixedVersionWriteKind.Change, key, incoming, now);
        }

        return now.Value - state.TouchedAt.Value >= touchRefresh
            ? new FixedVersionWrite(FixedVersionWriteKind.Touch, key, null, now)
            : null;
    }
}
