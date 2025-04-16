﻿using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using TrivyOperator.Dashboard.Infrastructure.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Services;

public class ConcurrentCache<TKey, TValue> : IConcurrentCache<TKey, TValue> where TKey : notnull
{
    public ConcurrentCache(IMetricsService metricsService)
    {
        metricsService.CreateObservableGauge(
            $"{metricsService.AppName}.cache.size",
            () => GetCacheMeasurements(),
            unit: "items",
            description: "Tracks the size of the caches."
        );
    }
    
    private readonly ConcurrentDictionary<TKey, TValue> dictionary = new();

    public TValue this[TKey key]
    {
        get => dictionary[key];
        set => dictionary[key] = value;
    }

    public IEnumerable<TKey> Keys => dictionary.Keys;

    public IEnumerable<TValue> Values => dictionary.Values;

    public int Count => dictionary.Count;

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) =>
        dictionary.TryGetValue(key, out value);

    public bool TryAdd(TKey key, TValue value) => dictionary.TryAdd(key, value);

    public bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue) =>
        dictionary.TryUpdate(key, newValue, comparisonValue);

    public bool TryRemove(TKey key, [MaybeNullWhen(false)] out TValue value) =>
        dictionary.TryRemove(key, out value);

    public bool ContainsKey(TKey key) => dictionary.ContainsKey(key);

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => dictionary.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => dictionary.GetEnumerator();

    public void Clear() => dictionary.Clear();

    protected virtual IEnumerable<Measurement<long>> GetCacheMeasurements()
    {
        List<Measurement<long>> measurements =
        [
            new Measurement<long>(
                dictionary.Count,
                new KeyValuePair<string, object?>("value_kind", "generic"),
                new KeyValuePair<string, object?>("value_type", typeof(TValue).Name)),
        ];

        return measurements;
    }
}
