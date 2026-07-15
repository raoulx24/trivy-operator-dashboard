using System.Diagnostics.CodeAnalysis;

namespace TrivyOperator.Dashboard.Infrastructure.Caching.InMemoryOld.Abstractions;

public interface IConcurrentCache<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
{
    new TValue this[TKey key] { get; set; }

    bool TryAdd(TKey key, TValue value);
    bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue);
    bool TryRemove(TKey key, [MaybeNullWhen(false)] out TValue value);
    void Clear();
}
