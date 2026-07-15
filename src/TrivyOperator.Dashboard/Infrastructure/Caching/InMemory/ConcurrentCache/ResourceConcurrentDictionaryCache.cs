using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Clients.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.ConcurrentCache;

public class ResourceResourceConcurrentDictionaryCache<TKey, TValue>(IMetricsClient metricsClient)
    : ConcurrentCache<NamespaceName, ConcurrentDictionary<TKey, TValue>>(metricsClient), IResourceConcurrentDictionaryCache<TKey, TValue>
where TKey : notnull
{
    protected override IEnumerable<Measurement<long>> GetCacheMeasurements()
    {
        List<Measurement<long>> measurements = [];
        measurements.AddRange(
            Keys.Select(key => new Measurement<long>(
                    this[key].Count,
                    new KeyValuePair<string, object?>("value_kind", "concurrent_dictionary"),
                    new KeyValuePair<string, object?>("value_type", typeof(TValue).Name),
                    new KeyValuePair<string, object?>("key_name", key)
                )
            )
        );

        return measurements;
    }
}
