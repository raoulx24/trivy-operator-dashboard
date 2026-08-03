using System.IO.Compression;
using System.Text.Json;
using TrivyOperator.Dashboard.Infrastructure.Persistence.CacheEntityCodec.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Persistence.CacheEntityCodec;

public sealed class JsonCacheEntityCodec : ICacheEntityCodec
{
    public byte[] Encode<T>(T data)
    {
        return JsonSerializer.SerializeToUtf8Bytes(data);
    }

    public T Decode<T>(byte[] data)
    {
        T? result = JsonSerializer.Deserialize<T>(data);

        return result ?? throw new JsonException(
            $"Failed to deserialize '{typeof(T).FullName}'.");
    }
}
