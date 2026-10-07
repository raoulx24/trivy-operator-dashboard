namespace TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;

public interface IExpiringResourceProvider<TResource, TKey> : IResourceProvider<TResource, TKey>
{
    Task Clear(CancellationToken ctx = default);
}
