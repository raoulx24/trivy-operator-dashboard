using MemoryPack;
using System.IO.Compression;
using TrivyOperator.Dashboard.Infrastructure.Persistence.CacheEntityCodec.Codecs.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Persistence.CacheEntityCodec.Codecs;

public sealed class BrotliMemoryPackCacheEntityCodec(
    CompressionLevel compressionLevel = CompressionLevel.Fastest)
    : ICacheEntityCodec
{
    public const string Name = "BrotliMemory";
    
    public byte[] Encode<T>(T data)
    {
        var bytes = MemoryPackSerializer.Serialize(data);

        using MemoryStream output = new();

        using (BrotliStream brotli = new BrotliStream(output, compressionLevel, true))
        {
            brotli.Write(bytes);
        }

        return output.ToArray();
    }

    public T Decode<T>(byte[] data)
    {
        using MemoryStream input = new(data);
        using BrotliStream brotli = new(input, CompressionMode.Decompress);
        using MemoryStream output = new();

        brotli.CopyTo(output);

        return MemoryPackSerializer.Deserialize<T>(output.ToArray())
               ?? throw new InvalidOperationException(
                   $"Failed to deserialize '{typeof(T).FullName}'.");
    }
    
    public string CodecName => Name;
}
