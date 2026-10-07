using k8s;
using k8s.Models;
using System.Threading.Channels;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Mappers.Abstract;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.PersistenceAggregators.Abstracts;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.PersistenceAggregators;

public abstract class ResourceAggregator<TKubernetesObject, TResource, TKey>(
    IResourceMapper<TKubernetesObject, TResource> mapper,
    IResourceKeyProvider<TKubernetesObject, TKey> keyProvider)
    : IResourceAggregator<TKubernetesObject, TResource, TKey>
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>
    where TResource : class, IEntity<TKey>
    where TKey : notnull
{
    public IReadOnlyDictionary<TKey, TResource> Aggregate(
        IEnumerable<TKubernetesObject> resources,
        CancellationToken cancellationToken = default)
    {
        Dictionary<TKey, TResource> reports = new();

        foreach (TKubernetesObject cr in resources)
        {
            cancellationToken.ThrowIfCancellationRequested();

            TKey key = keyProvider.GetKey(cr);

            TResource? existing = ResolveExisting(key, reports);

            TResource report = mapper.MapToDomain(cr, existing);

            reports[key] = report;
        }

        return reports;
    }
    
    public async Task<IReadOnlyDictionary<TKey, TResource>> AggregateFromChannelAsync(
        ChannelReader<TKubernetesObject> reader,
        CancellationToken cancellationToken = default)
    {
        Dictionary<TKey, TResource> reports = new();

        await foreach (TKubernetesObject cr in reader.ReadAllAsync(cancellationToken))
        {
            TKey key = keyProvider.GetKey(cr);

            TResource? existing = ResolveExisting(key, reports);

            TResource report = mapper.MapToDomain(cr, existing);

            reports[key] = report;
        }

        return reports;
    }

    protected virtual TResource? ResolveExisting(TKey key, Dictionary<TKey, TResource> reports) => null;
}