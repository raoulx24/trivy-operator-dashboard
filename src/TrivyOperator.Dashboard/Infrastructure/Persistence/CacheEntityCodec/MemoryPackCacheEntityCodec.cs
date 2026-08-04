using MemoryPack;
using TrivyOperator.Dashboard.Infrastructure.Persistence.CacheEntityCodec.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Persistence.CacheEntityCodec;

public sealed class MemoryPackCacheEntityCodec : ICacheEntityCodec
{
    public byte[] Encode<T>(T data)
    {
        return MemoryPackSerializer.Serialize(data);
    }

    public T Decode<T>(byte[] data)
    {
        return MemoryPackSerializer.Deserialize<T>(data)
               ?? throw new InvalidOperationException(
                   $"Failed to deserialize '{typeof(T).FullName}'.");
    }
}
