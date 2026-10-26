using System.Collections.Concurrent;
using TrivyOperator.Dashboard.Application.CveEnrichment.Abstractions;
using TrivyOperator.Dashboard.Domain.CveEnrichment.ValueObjects;

namespace TrivyOperator.Dashboard.Application.CveEnrichment.Services;

// one signal per source - a single pending request per source is enough, multiple requests collapse into one
public sealed class CveEnrichmentRefreshTrigger : ICveEnrichmentRefreshTrigger, IDisposable
{
    private readonly ConcurrentDictionary<CveSource, SemaphoreSlim> signals = new()
    {
        [CveSources.Kev] = new SemaphoreSlim(0, 1),
        [CveSources.Epss] = new SemaphoreSlim(0, 1),
        [CveSources.Euvd] = new SemaphoreSlim(0, 1),
    };

    public void RequestRebuild()
    {
        foreach (SemaphoreSlim signal in signals.Values)
        {
            try
            {
                signal.Release();
            }
            catch (SemaphoreFullException)
            {
                // already requested
            }
        }
    }

    public Task<bool> WaitAsync(CveSource source, TimeSpan timeout, CancellationToken ctx = default) =>
        signals.GetOrAdd(source, static _ => new SemaphoreSlim(0, 1)).WaitAsync(timeout, ctx);

    public void Dispose()
    {
        foreach (SemaphoreSlim signal in signals.Values)
        {
            signal.Dispose();
        }
    }
}
