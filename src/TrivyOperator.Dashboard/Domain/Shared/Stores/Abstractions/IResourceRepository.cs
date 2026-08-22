namespace TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;

public interface IResourceRepository<TResource, TKey> : IResourceStore<TResource, TKey>, IResourceProvider<TResource, TKey>
{
    
}
