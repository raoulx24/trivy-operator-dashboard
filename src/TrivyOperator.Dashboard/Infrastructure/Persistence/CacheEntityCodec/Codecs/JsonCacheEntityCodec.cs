using System.Text.Json;
using TrivyOperator.Dashboard.Infrastructure.Persistence.CacheEntityCodec.Codecs.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Persistence.CacheEntityCodec.Codecs;

public sealed class JsonCacheEntityCodec : ICacheEntityCodec
{
    public const string Name = "Json";
    
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

    public string CodecName => Name;
}
