using TrivyOperator.Dashboard.Infrastructure.Caching.CacheEntityCodec.Codecs;
using TrivyOperator.Dashboard.Infrastructure.Caching.CacheEntityCodec.Codecs.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.CacheEntityCodec.Factories.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Caching.CacheEntityCodec.Factories;

public class CacheEntityCodecFactory : ICacheEntityCodecFactory
{
    private static readonly JsonCacheEntityCodec InternalJsonCacheEntityCodec = new();
    private static readonly MemoryPackCacheEntityCodec InternalMemoryPackCacheEntityCodec = new();
    private static readonly BrotliJsonCacheEntityCodec InternalBrotliJsonCacheEntityCodec = new();
    private static readonly BrotliMemoryPackCacheEntityCodec InternalBrotliMemoryPackCacheEntityCodec = new();
    
    public ICacheEntityCodec GetCacheEntityCodec(string codecName)
    {
        return codecName switch
        {
            JsonCacheEntityCodec.Name => InternalJsonCacheEntityCodec,

            MemoryPackCacheEntityCodec.Name => InternalMemoryPackCacheEntityCodec,

            BrotliJsonCacheEntityCodec.Name => InternalBrotliJsonCacheEntityCodec,

            BrotliMemoryPackCacheEntityCodec.Name => InternalBrotliMemoryPackCacheEntityCodec,

            _ => throw new ArgumentException(
                $"Provided codec name is invalid: {codecName}",
                nameof(codecName)),
        };
    }
}
