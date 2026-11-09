using TrivyOperator.Dashboard.Domain.History.FixedVersionHistory.Entities;
using TrivyOperator.Dashboard.Domain.History.FixedVersionHistory.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Vulnerabilities;

namespace TrivyOperator.Dashboard.Infrastructure.History.Stores;

public class DistributedCacheFixedVersionHistoryNullStore : IFixedVersionHistoryStore
{
    public Task<IReadOnlyDictionary<PackageVulnerabilityKey, FixedVersionState>> GetStatesAsync(
        IReadOnlyCollection<PackageVulnerabilityKey> keys,
        CancellationToken ctx = default)
        => Task.FromResult<IReadOnlyDictionary<PackageVulnerabilityKey, FixedVersionState>>(
            new Dictionary<PackageVulnerabilityKey, FixedVersionState>());

    public Task<IReadOnlyDictionary<PackageVulnerabilityKey, FixedVersionTimeline>> GetTimelinesAsync(
        IReadOnlyCollection<PackageVulnerabilityKey> keys,
        CancellationToken ctx = default)
        => Task.FromResult<IReadOnlyDictionary<PackageVulnerabilityKey, FixedVersionTimeline>>(
            new Dictionary<PackageVulnerabilityKey, FixedVersionTimeline>());

    public Task ApplyAsync(IReadOnlyCollection<FixedVersionWrite> writes, CancellationToken ctx = default)
        => Task.CompletedTask;

    public Task<Timestamp?> GetTrackingStartedAtAsync(CancellationToken ctx = default)
        => Task.FromResult<Timestamp?>(null);

    public Task<IReadOnlyList<FixedVersionEntry>> GetEntriesAsync(
        PackageVulnerabilityKey key,
        CancellationToken ctx = default)
        => Task.FromResult<IReadOnlyList<FixedVersionEntry>>([]);

    public Task<IReadOnlyList<FixedVersionChange>> GetChangesAsync(Timestamp from, CancellationToken ctx = default)
        => Task.FromResult<IReadOnlyList<FixedVersionChange>>([]);

    public Task<IReadOnlyList<FixedVersionState>> GetAllStatesAsync(CancellationToken ctx = default)
        => Task.FromResult<IReadOnlyList<FixedVersionState>>([]);

    public Task<bool> DeleteIfUntouchedAsync(FixedVersionState state, CancellationToken ctx = default)
        => Task.FromResult(false);
}
