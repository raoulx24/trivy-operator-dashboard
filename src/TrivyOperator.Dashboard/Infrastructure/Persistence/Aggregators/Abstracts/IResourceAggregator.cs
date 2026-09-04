using k8s;
using k8s.Models;
using System.Threading.Channels;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Persistence.Aggregators.Abstracts;

public interface IResourceAggregator<TKubernetesObject, TResource, TKey>
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>
    where TResource : class, IEntity<TKey>
    where TKey : notnull
{
    IReadOnlyDictionary<TKey, TResource> Aggregate(
        IEnumerable<TKubernetesObject> resources,
        CancellationToken cancellationToken = default
    );
    
    Task<IReadOnlyDictionary<TKey, TResource>> AggregateFromChannelAsync(
        ChannelReader<TKubernetesObject> reader,
        CancellationToken cancellationToken = default);
}
