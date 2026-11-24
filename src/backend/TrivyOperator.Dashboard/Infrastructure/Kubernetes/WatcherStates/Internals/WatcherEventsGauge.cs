using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.WatcherStates.Internals;

public class WatcherEventsGauge
{
    private readonly Dictionary<WatcherScope, int> data = [];

    public void SetValue(WatcherScope key, int value) => data[key] = value;

    public void OffsetValue(WatcherScope key, int offset)
    {
        if (offset == 0)
        {
            return;
        }

        if (!data.TryAdd(key, offset))
        {
            data[key] += offset;
        }
    }

    public bool RemoveKey(WatcherScope key) => data.Remove(key);

    public int? GetValue(WatcherScope key) => data.TryGetValue(key, out int value) ? value : null;

    public void Clear() => data.Clear();
}
