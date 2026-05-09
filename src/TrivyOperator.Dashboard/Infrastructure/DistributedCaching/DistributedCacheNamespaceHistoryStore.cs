using TrivyOperator.Dashboard.Domain.History.NamespaceHistory.Abstractions;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.DistributedCaching.Client;
using TrivyOperator.Dashboard.Infrastructure.DistributedCaching.Client.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.DistributedCaching;

public class DistributedCacheNamespaceHistoryStore(
    IDistributedCacheExecutor executor,
    ILogger<DistributedCacheNamespaceHistoryStore> logger
): INamespaceHistoryStore
{
    public async Task<IReadOnlyList<NamespaceName>> GetNamespacesAsync(
        CancellationToken ct = default)
    {
        return await executor.ExecuteAsync(async db =>
        {
            IReadOnlyList<string> values =
                await DistributedCachePrimitives.GetSetMembersAsync(
                    db,
                    DistributedCacheKeyExtensions.NamespacesKey,
                    logger,
                    ct);
            
            List<NamespaceName> result = [];

            foreach (string value in values)
            {
                try
                {
                    result.Add(new NamespaceName(value));
                }
                catch (Exception ex)
                {
                    logger.LogError(ex,
                        "Invalid namespace name '{namespaceName}' in distributed cache",
                        value);
                }
            }

            return result;
        }, ct);
    }

    public async Task AddOrUpdateNamespaceAsync(
        NamespaceName namespaceName,
        CancellationToken ct = default)
    {
        await executor.ExecuteAsync(async db =>
        {
            await DistributedCachePrimitives.AddToSetAsync(
                db,
                DistributedCacheKeyExtensions.NamespacesKey,
                namespaceName.Value,
                ct);

            return true;
        }, ct);
    }

    public async Task DeleteNamespacesAsync(
        IEnumerable<NamespaceName> namespaceNames,
        CancellationToken ct = default)
    {
        await executor.ExecuteAsync(async db =>
        {
            await DistributedCachePrimitives.RemoveFromSetAsync(
                db,
                DistributedCacheKeyExtensions.NamespacesKey,
                namespaceNames.Select(x => x.Value),
                ct);

            return true;
        }, ct);
    }
}
