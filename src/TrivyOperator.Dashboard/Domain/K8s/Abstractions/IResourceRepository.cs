namespace TrivyOperator.Dashboard.Domain.K8s.Abstractions;

public interface IResourceRepository<TResource, in TKey> : IResourceStore<TResource, TKey>, IResourceProvider<TResource>
{
    
}
