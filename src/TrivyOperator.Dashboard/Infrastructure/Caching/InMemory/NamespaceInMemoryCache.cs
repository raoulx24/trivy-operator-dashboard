using TrivyOperator.Dashboard.Domain.K8s.Entities;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.ResourceStore.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Caching.InMemory;

public class NamespaceInMemoryCache(IResourceConcurrentDictionaryCache<NamespaceName, K8sNamespace> cache) : IResourceStore<K8sNamespace>
{
    public Task UpsertResource(NamespaceName namespaceName, K8sNamespace resource) => throw new NotImplementedException();

    public Task DeleteResource(NamespaceName namespaceName, K8sNamespace resource) => throw new NotImplementedException();
}
