using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

namespace TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;

public interface IDetailStore<TDetail, TId>
    where TDetail : IDetail<TId>
{
    // adds the detail to the current context, or, when it exists:
    // - replaces the content (and LastSeenAt) only when lastSeenAt is newer than the stored one
    // - refreshes TouchedAt when it is older than the touch refresh interval; otherwise nothing is written
    Task Upsert(TDetail detail, Timestamp lastSeenAt, CancellationToken ctx = default);

    // housekeeping: removes the entry only if it is still the instance the caller saw
    // (an Upsert in the meantime replaced it, so the removal is skipped)
    Task<bool> RemoveIf(
        ContextName contextName,
        TId id,
        DetailEntry<TDetail> expected,
        CancellationToken ctx = default);
}
