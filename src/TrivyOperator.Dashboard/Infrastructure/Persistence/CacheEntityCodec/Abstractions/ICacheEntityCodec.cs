namespace TrivyOperator.Dashboard.Infrastructure.Persistence.CacheEntityCodec.Abstractions;

public interface ICacheEntityCodec
{
    byte[] Encode<T>(T data);
    T Decode<T>(byte[] data);
}
