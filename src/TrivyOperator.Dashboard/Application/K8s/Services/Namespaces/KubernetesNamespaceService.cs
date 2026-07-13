using k8s.Models;
using System.Collections.Concurrent;
using TrivyOperator.Dashboard.Application.K8s.Services.Namespaces.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Utils;

namespace TrivyOperator.Dashboard.Application.K8s.Services.Namespaces;

public class KubernetesNamespaceService(IConcurrentDictionaryCache<V1Namespace> cache) : IKubernetesNamespaceService
{
    public Task<IEnumerable<string>> GetKubernetesNamespaces()
    {
        IEnumerable<string> namespaceNames = [];
        if (cache.TryGetValue(
                CacheUtils.DefaultCacheRefreshKey,
                out ConcurrentDictionary<string, V1Namespace>? namespacesCache
            ))
        {
            namespaceNames = [.. namespacesCache.Values.Select(x => x.Metadata.Name),];
        }

        return Task.FromResult(namespaceNames);
    }
}
