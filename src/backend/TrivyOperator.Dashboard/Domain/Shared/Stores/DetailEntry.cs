using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

namespace TrivyOperator.Dashboard.Domain.Shared.Stores;

// immutable, replaced (never changed in place) so a conditional removal can compare by reference
public sealed class DetailEntry<TDetail>
{
    public required TDetail Value { get; init; }

    // the LastSeenAt of the entity that supplied Value; orders the content (newer wins)
    public required Timestamp LastSeenAt { get; init; }

    // when the entry was last written by an event; the housekeeping grace period is measured from it
    public required Timestamp TouchedAt { get; init; }
}
