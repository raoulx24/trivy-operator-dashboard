using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Application.Queries.History.Shared;

public static class HistoryQuerySupport
{
    public static async Task<IReadOnlyList<SnapshotIndexEntry>> GetResourceIndexes(
        IVulnerabilityReportsHistoryStore vrHistoryStore,
        Digest digest,
        string? namespaceName,
        CancellationToken ctx = default)
    {
        IReadOnlyList<SnapshotIndexEntry> snapshotIndexes =
            await vrHistoryStore.GetSnapshotIndexesAsync(digest, ctx);

        NamespaceName ns = new(namespaceName);

        // filter summaries first to avoid fetching reports we don't need
        return 
        [
            .. snapshotIndexes
                .Where(x => namespaceName is null || x.HistoryMetadata.HasNamespaceName(ns)),
        ];
    }
}
