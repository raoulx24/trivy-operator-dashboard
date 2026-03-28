namespace TrivyOperator.Dashboard.Infrastructure.Utils;

public class DictionaryCounter
{
    private readonly Dictionary<string, int> data = [];

    public void SetValue(string key, int value) => data[key] = value;

    public void OffsetValue(string key, int offset)
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

    public bool RemoveKey(string key) => data.Remove(key);

    public int? GetValue(string key) => data.TryGetValue(key, out int value) ? value : null;

    public void Clear() => data.Clear();
}
