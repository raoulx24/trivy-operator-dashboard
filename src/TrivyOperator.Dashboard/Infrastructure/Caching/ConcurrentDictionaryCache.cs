using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using TrivyOperator.Dashboard.Infrastructure.Caching.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Clients.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Caching;

public class ConcurrentDictionaryCache<TValue>(IMetricsClient metricsClient)
    : ConcurrentCache<string, ConcurrentDictionary<string, TValue>>(metricsClient), IConcurrentDictionaryCache<TValue>
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
