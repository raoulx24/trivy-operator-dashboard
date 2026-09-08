using TrivyOperator.Dashboard.Infrastructure.Caching.CacheEntityCodec.Codecs.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Caching.CacheEntityCodec.Factories.Abstractions;

public interface ICacheEntityCodecFactory
{
    ICacheEntityCodec GetCacheEntityCodec(string codecName);
}
