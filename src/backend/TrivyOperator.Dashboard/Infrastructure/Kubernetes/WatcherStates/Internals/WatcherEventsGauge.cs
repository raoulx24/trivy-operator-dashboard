using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.WatcherStates.Internals;

public class WatcherEventsGauge
{
    private readonly Dictionary<ResourceLocation, int> data = [];

    public void SetValue(ResourceLocation key, int value) => data[key] = value;

    public void OffsetValue(ResourceLocation key, int offset)
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

    public bool RemoveKey(ResourceLocation key) => data.Remove(key);

    public int? GetValue(ResourceLocation key) => data.TryGetValue(key, out int value) ? value : null;

    public void Clear() => data.Clear();
}
