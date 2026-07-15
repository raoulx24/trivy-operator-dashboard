using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

namespace TrivyOperator.Dashboard.Infrastructure.ResourceStore.Abstractions;

public interface IResourceStore<in T>
{
    Task UpsertResource(NamespaceName namespaceName, T resource);

    Task DeleteResource(NamespaceName namespaceName, T resource);
}
