using System.Reflection;
using TrivyOperator.Dashboard.Application.K8s.Services.RawDomainQuery.Abstracts;
using TrivyOperator.Dashboard.Infrastructure.Caching.Abstractions;

namespace TrivyOperator.Dashboard.Application.K8s.Services.RawDomainQuery;

public sealed class RawDomainQueryService(IServiceProvider sp) : IRawDomainQueryService
{
    public Task<IReadOnlyList<object>> GetAllAsync(Type valueType, string key, CancellationToken ct = default)
    {
        var closedType = typeof(IConcurrentDictionaryCache<>).MakeGenericType(valueType);

        var cacheObj = sp.GetServices(closedType).FirstOrDefault()
            ?? throw new CacheNotRegisteredException($"No cache registered for {valueType}.");

        return Task.FromResult(ExtractValues(valueType, cacheObj, key));
    }

    private static IReadOnlyList<object> ExtractValues(Type valueType, object cacheObj, string key)
    {
        // Strong-typed generic call via reflection
        var method = typeof(RawDomainQueryService)
            .GetMethod(nameof(ExtractValuesGeneric), BindingFlags.Static | BindingFlags.NonPublic)!
            .MakeGenericMethod(valueType);

        return (IReadOnlyList<object>)method.Invoke(null, [cacheObj, key])!;
    }

#pragma warning disable CA1859 // Use concrete types when possible for improved performance
    private static IReadOnlyList<object> ExtractValuesGeneric<T>(object cacheObj, string key)
#pragma warning restore CA1859 // Use concrete types when possible for improved performance
    {
        var cache = (IConcurrentDictionaryCache<T>)cacheObj;

        if (cache.TryGetValue(key, out var dict))
            return [.. dict.Values.Cast<object>()];

        return [];
    }
}