namespace TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;

public interface IExpiringResourceRepository<TResource, TKey> : IResourceStore<TResource, TKey>, IExpiringResourceProvider<TResource, TKey>
{
    
}
