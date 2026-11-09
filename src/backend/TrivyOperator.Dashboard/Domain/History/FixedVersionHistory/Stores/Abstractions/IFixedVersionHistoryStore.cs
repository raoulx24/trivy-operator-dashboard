using TrivyOperator.Dashboard.Domain.History.FixedVersionHistory.Entities;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Vulnerabilities;

namespace TrivyOperator.Dashboard.Domain.History.FixedVersionHistory.Stores.Abstractions;

// the history of the fixed version per (vulnerability id, package), global (not per image)
public interface IFixedVersionHistoryStore
{
    // the states of the given pairs (missing pairs are not in the result), in one round trip
    Task<IReadOnlyDictionary<PackageVulnerabilityKey, FixedVersionState>> GetStatesAsync(
        IReadOnlyCollection<PackageVulnerabilityKey> keys,
        CancellationToken ctx = default);

    // applies the recorder's decisions, in one round trip
    Task ApplyAsync(IReadOnlyCollection<FixedVersionWrite> writes, CancellationToken ctx = default);

    // the timelines of the given pairs (missing pairs are not in the result), derived from their entries, in one
    // round trip
    Task<IReadOnlyDictionary<PackageVulnerabilityKey, FixedVersionTimeline>> GetTimelinesAsync(
        IReadOnlyCollection<PackageVulnerabilityKey> keys,
        CancellationToken ctx = default);

    // the moment the history started recording (set once, with the first write); null when nothing was written yet
    // the entries carry the reports' moments, so a baseline older than this is a pair that existed before the
    // recording started (its age is unknown), not one first seen then
    Task<Timestamp?> GetTrackingStartedAtAsync(CancellationToken ctx = default);

    // all entries of a pair, oldest first (the first one is the baseline)
    Task<IReadOnlyList<FixedVersionEntry>> GetEntriesAsync(PackageVulnerabilityKey key, CancellationToken ctx = default);

    // the changes (not the baselines) at or after from, oldest first
    Task<IReadOnlyList<FixedVersionChange>> GetChangesAsync(Timestamp from, CancellationToken ctx = default);

    // all pairs with their touch, for the housekeeping
    Task<IReadOnlyList<FixedVersionState>> GetAllStatesAsync(CancellationToken ctx = default);

    // housekeeping: deletes the pair (and its changes) only if it was not touched since the state was read
    Task<bool> DeleteIfUntouchedAsync(FixedVersionState state, CancellationToken ctx = default);
}
