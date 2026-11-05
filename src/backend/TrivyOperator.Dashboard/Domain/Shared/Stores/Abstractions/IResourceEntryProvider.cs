namespace TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;

// the entries as they are stored (e.g. the slim reports) of the current context, without rebuilding the resources
public interface IResourceEntryProvider<TEntry, TKey>
{
    Task<IReadOnlyList<TEntry>> GetEntries(CancellationToken ctx = default);
}
