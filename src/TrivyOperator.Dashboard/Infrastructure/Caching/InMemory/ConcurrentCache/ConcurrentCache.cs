using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Clients.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.ConcurrentCache;

public class ConcurrentCache<TKey, TValue> : IConcurrentCache<TKey, TValue>
    where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, TValue> dictionary = new();

    public ConcurrentCache(IMetricsClient metricsClient)
    {
        metricsClient.CreateObservableGauge(
            $"{metricsClient.AppName}.cache.size",
            GetCacheMeasurements,
            "items",
            "Tracks the size of the caches."
        );
    }

    public TValue this[TKey key]
    {
        get
        {
            OnAccess();
            return dictionary[key];
        }
        set
        {
            OnAccess();
            dictionary[key] = value;
        }
    }

    public IEnumerable<TKey> Keys
    {
        get
        {
            OnAccess();
            return dictionary.Keys;
        }
    }

    public IEnumerable<TValue> Values
    {
        get
        {
            OnAccess();
            return dictionary.Values;
        }
    }

    public int Count
    {
        get
        {
            OnAccess();
            return dictionary.Count;
        }
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        OnAccess();
        return dictionary.TryGetValue(key, out value);
    }

    public bool TryAdd(TKey key, TValue value)
    {
        OnAccess();
        return dictionary.TryAdd(key, value);
    }

    public bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue)
    {
        OnAccess();
        return dictionary.TryUpdate(key, newValue, comparisonValue);
    }

    public bool TryRemove(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        OnAccess();
        return dictionary.TryRemove(key, out value);
    }

    public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
    {
        OnAccess();
        return dictionary.GetOrAdd(key, valueFactory);
    }

    public bool ContainsKey(TKey key)
    {
        OnAccess();
        return dictionary.ContainsKey(key);
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        OnAccess();
        return dictionary.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        OnAccess();
        return dictionary.GetEnumerator();
    }

    public void Clear()
    {
        OnClear();
        dictionary.Clear();
    }

    protected virtual IEnumerable<Measurement<long>> GetCacheMeasurements()
    {
        List<Measurement<long>> measurements =
        [
            new(
                GetCountForMetrics(),
                new KeyValuePair<string, object?>("value_kind", "generic"),
                new KeyValuePair<string, object?>("value_type", typeof(TValue).Name)
            ),
        ];

        return measurements;
    }

    protected TValue GetValueForMetrics(TKey key)
    {
        return dictionary[key];
    }

    protected IEnumerable<TKey> GetKeysForMetrics()
    {
        return dictionary.Keys;
    }

    protected int GetCountForMetrics()
    {
        return dictionary.Count;
    }
    
    protected virtual void OnAccess() { }
    protected virtual void OnClear() { }
}
