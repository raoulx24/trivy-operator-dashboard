using System.Collections.Concurrent;
using System.Reflection;
using TrivyOperator.Dashboard.Application.K8s.Services.RawDomain.Abstracts;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.Abstractions;

namespace TrivyOperator.Dashboard.Application.K8s.Services.RawDomain;

public sealed class RawDomainQueryService(IServiceProvider sp) : IRawDomainQueryService
{
    public Task<IReadOnlyList<object>> GetAllAsync(Type valueType, string key, CancellationToken ct = default)
    {
        Type closedType = typeof(IConcurrentDictionaryCache<>).MakeGenericType(valueType);

        object cacheObj = sp.GetServices(closedType).FirstOrDefault() ??
                          throw new CacheNotRegisteredException($"No cache registered for {valueType}.");

        return Task.FromResult(ExtractValues(valueType, cacheObj, key));
    }

    private static IReadOnlyList<object> ExtractValues(Type valueType, object cacheObj, string key)
    {
        // Strong-typed generic call via reflection
        MethodInfo method = typeof(RawDomainQueryService).GetMethod(
            nameof(ExtractValuesGeneric),
            BindingFlags.Static | BindingFlags.NonPublic
        )!.MakeGenericMethod(valueType);

        return (IReadOnlyList<object>)method.Invoke(null, [cacheObj, key,])!;
    }

#pragma warning disable CA1859 // Use concrete types when possible for improved performance
    private static IReadOnlyList<object> ExtractValuesGeneric<T>(object cacheObj, string key)
#pragma warning restore CA1859 // Use concrete types when possible for improved performance
    {
        IConcurrentDictionaryCache<T> cache = (IConcurrentDictionaryCache<T>)cacheObj;

        if (cache.TryGetValue(key, out ConcurrentDictionary<string, T>? dict))
        {
            return [.. dict.Values.Cast<object>(),];
        }

        return [];
    }
}
