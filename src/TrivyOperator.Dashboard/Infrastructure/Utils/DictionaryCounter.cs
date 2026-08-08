using TrivyOperator.Dashboard.Application.K8s.Models.WatcherEvents;

namespace TrivyOperator.Dashboard.Infrastructure.Utils;

public class DictionaryCounter
{
    private readonly Dictionary<WatcherKey, int> data = [];

    public void SetValue(WatcherKey key, int value) => data[key] = value;

    public void OffsetValue(WatcherKey key, int offset)
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

    public bool RemoveKey(WatcherKey key) => data.Remove(key);

    public int? GetValue(WatcherKey key) => data.TryGetValue(key, out int value) ? value : null;

    public void Clear() => data.Clear();
}
