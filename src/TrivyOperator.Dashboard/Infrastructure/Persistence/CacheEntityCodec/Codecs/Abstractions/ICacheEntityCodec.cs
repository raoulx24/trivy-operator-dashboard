namespace TrivyOperator.Dashboard.Infrastructure.Persistence.CacheEntityCodec.Codecs.Abstractions;

public interface ICacheEntityCodec
{
    byte[] Encode<T>(T data);
    T Decode<T>(byte[] data);
    string CodecName { get; }
}
