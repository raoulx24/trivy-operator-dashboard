using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;

namespace TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;

public interface IDetailProvider<TDetail, TId>
    where TDetail : IDetail<TId>
    where TId : notnull
{
    // the details of the current context; ids without a detail are absent from the result
    Task<IReadOnlyDictionary<TId, TDetail>> GetDetails(IEnumerable<TId> ids, CancellationToken ctx = default);

    // housekeeping: a snapshot of the entries of all the contexts
    Task<IReadOnlyDictionary<ContextName, IReadOnlyDictionary<TId, DetailEntry<TDetail>>>> GetEntries(
        CancellationToken ctx = default);
}
