using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;

namespace TrivyOperator.Dashboard.Application.Shared.Details.Providers;

// gives the query services the full reports back: the stored slim reports plus their details from the details store(s)
// - the summaries are the reports without details, as before
public abstract class HydratingResourceProvider<TReport, TSlim, TKey>(
    IResourceProvider<TSlim, TKey> slimProvider
) : IResourceProvider<TReport, TKey>
    where TReport : class
    where TSlim : class
{
    public async Task<TReport?> GetResource(TKey key, CancellationToken ctx = default)
    {
        TSlim? slim = await slimProvider.GetResource(key, ctx);

        if (slim is null)
        {
            return null;
        }

        IReadOnlyList<TReport> result = await Hydrate([slim,], ctx);

        return result[0];
    }

    public async Task<IReadOnlyList<TReport>> GetResources(CancellationToken ctx = default)
    {
        return await Hydrate(await slimProvider.GetResources(ctx), ctx);
    }

    public async Task<IReadOnlyList<TReport>> GetResources(IEnumerable<TKey> keys, CancellationToken ctx = default)
    {
        return await Hydrate(await slimProvider.GetResources(keys, ctx), ctx);
    }

    public async Task<TReport?> GetResourceSummary(TKey key, CancellationToken ctx = default)
    {
        TSlim? slim = await slimProvider.GetResourceSummary(key, ctx);

        return slim is null ? null : ToSummary(slim);
    }

    public async Task<IReadOnlyList<TReport>> GetResourceSummaries(CancellationToken ctx = default)
    {
        IReadOnlyList<TSlim> slims = await slimProvider.GetResourceSummaries(ctx);

        return [.. slims.Select(ToSummary),];
    }

    public Task<IReadOnlyList<TKey>> GetResourceIds(CancellationToken ctx = default)
    {
        return slimProvider.GetResourceIds(ctx);
    }

    // the full reports, in the order of the slims; the details are read once for all the slims
    protected abstract Task<IReadOnlyList<TReport>> Hydrate(IReadOnlyList<TSlim> slims, CancellationToken ctx);

    // the report without details
    protected abstract TReport ToSummary(TSlim slim);
}
