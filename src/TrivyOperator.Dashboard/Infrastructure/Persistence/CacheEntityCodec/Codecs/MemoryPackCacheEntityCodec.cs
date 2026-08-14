using MemoryPack;
using TrivyOperator.Dashboard.Infrastructure.Persistence.CacheEntityCodec.Codecs.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Persistence.CacheEntityCodec.Codecs;

public sealed class MemoryPackCacheEntityCodec : ICacheEntityCodec
{
    public const string Name = "MemoryPack";
    
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
    
    public string CodecName => Name;
}
