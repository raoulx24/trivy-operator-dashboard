using k8s;
using k8s.Models;
using System.Collections;
using System.Collections.Concurrent;
using TrivyOperator.Dashboard.Infrastructure.Caching.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Utils;

namespace TrivyOperator.Dashboard.Infrastructure.Caching;

public abstract class ResourcePassthroughCache<TValue>()
    : IConcurrentDictionaryCache<TValue>
    where TValue : IKubernetesObject<V1ObjectMeta>, IMetadata<V1ObjectMeta>
{
    protected abstract Task<IList<TValue>> FetchAllAsync(CancellationToken? cancellationToken = null);
    protected abstract Task<IList<TValue>> FetchByKeyAsync(string key, CancellationToken? cancellationToken = null);

    protected static ConcurrentDictionary<string, TValue> BuildInner(IList<TValue> items)
        => new(items.ToDictionary(x => x.Uid(), x => x));

    protected static IDictionary<string, ConcurrentDictionary<string, TValue>> BuildOuter(IList<TValue> items)
        => items
            .GroupBy(item => CacheUtils.GetCacheRefreshKey(item))
            .ToDictionary(
                g => g.Key,
                g => BuildInner(g.ToList())
            );

    // ---------------------------------------------------------
    // Public indexer (satisfies IReadOnlyDictionary)
    // ---------------------------------------------------------
    public ConcurrentDictionary<string, TValue> this[string key]
    {
        get
        {
            var items = FetchByKeyAsync(key).Result;
            return BuildInner(items);
        }
        set => throw new NotSupportedException("Cache is read-only");
    }

    // -------------------------------------------------------------------
    // Explicit indexer (satisfies IConcurrentCache<string, ConcurrentDictionary<string,TValue>>)
    // -------------------------------------------------------------------
    ConcurrentDictionary<string, TValue> IConcurrentCache<string, ConcurrentDictionary<string, TValue>>.this[string key]
    {
        get => this[key];
        set => throw new NotSupportedException("Cache is read-only");
    }

    // -----------------------
    // IReadOnlyDictionary impl
    // -----------------------
    public IEnumerable<string> Keys
        => BuildOuter(FetchAllAsync().Result).Keys;

    public IEnumerable<ConcurrentDictionary<string, TValue>> Values
        => BuildOuter(FetchAllAsync().Result).Values;

    public int Count
        => BuildOuter(FetchAllAsync().Result).Count;

    public bool ContainsKey(string key)
        => BuildOuter(FetchAllAsync().Result).ContainsKey(key);

    public bool TryGetValue(string key, out ConcurrentDictionary<string, TValue> value)
    {
        var items = FetchByKeyAsync(key).Result;
        value = BuildInner(items);
        return value.Count > 0;
    }

    // -----------------------
    // Enumerators (correct!)
    // -----------------------
    public IEnumerator<KeyValuePair<string, ConcurrentDictionary<string, TValue>>> GetEnumerator()
        => BuildOuter(FetchAllAsync().Result).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    // -----------------------
    // Mutating ops
    // -----------------------
    public bool TryAdd(string key, ConcurrentDictionary<string, TValue> value) => false;

    public bool TryUpdate(string key, ConcurrentDictionary<string, TValue> newValue, ConcurrentDictionary<string, TValue> comparisonValue) => false;

    public bool TryRemove(string key, out ConcurrentDictionary<string, TValue> value)
    {
        value = null!;
        return false;
    }

    public void Clear() { }
}
