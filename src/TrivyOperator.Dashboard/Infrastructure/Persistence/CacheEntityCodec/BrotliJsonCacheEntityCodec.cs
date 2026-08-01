using System.IO.Compression;
using System.Text.Json;
using TrivyOperator.Dashboard.Infrastructure.Persistence.CacheEntityCodec.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Persistence.CacheEntityCodec;

public sealed class BrotliJsonCacheEntityCodec(CompressionLevel compressionLevel = CompressionLevel.Fastest) : ICacheEntityCodec
{
    public byte[] Encode<T>(T data)
    {
        using MemoryStream output = new();

        using (BrotliStream brotli = new(output, compressionLevel, leaveOpen: true))
        {
            JsonSerializer.Serialize(brotli, data);
        }

        return output.ToArray();
    }

    public T Decode<T>(byte[] data)
    {
        using MemoryStream input = new(data);
        using BrotliStream brotli = new(input, CompressionMode.Decompress);

        T? result = JsonSerializer.Deserialize<T>(brotli);

        return result ?? throw new JsonException(
            $"Failed to deserialize '{typeof(T).FullName}'.");
    }
}