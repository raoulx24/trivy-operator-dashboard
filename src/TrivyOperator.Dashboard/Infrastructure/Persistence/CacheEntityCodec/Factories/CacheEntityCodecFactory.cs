using TrivyOperator.Dashboard.Infrastructure.Persistence.CacheEntityCodec.Codecs;
using TrivyOperator.Dashboard.Infrastructure.Persistence.CacheEntityCodec.Codecs.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Persistence.CacheEntityCodec.Factories.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Persistence.CacheEntityCodec.Factories;

public class CacheEntityCodecFactory : ICacheEntityCodecFactory
{
    public ICacheEntityCodec GetCacheEntityCodec(string codecName)
    {
        return codecName switch
        {
            JsonCacheEntityCodec.Name =>
                new JsonCacheEntityCodec(),

            MemoryPackCacheEntityCodec.Name =>
                new MemoryPackCacheEntityCodec(),

            BrotliJsonCacheEntityCodec.Name =>
                new BrotliJsonCacheEntityCodec(),

            BrotliMemoryPackCacheEntityCodec.Name =>
                new BrotliMemoryPackCacheEntityCodec(),

            _ => throw new ArgumentException(
                $"Provided codec name is invalid: {codecName}",
                nameof(codecName)),
        };
    }
}
