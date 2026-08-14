using TrivyOperator.Dashboard.Infrastructure.Persistence.CacheEntityCodec.Codecs.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Persistence.CacheEntityCodec.Factories.Abstractions;

public interface ICacheEntityCodecFactory
{
    ICacheEntityCodec GetCacheEntityCodec(string codecName);
}
