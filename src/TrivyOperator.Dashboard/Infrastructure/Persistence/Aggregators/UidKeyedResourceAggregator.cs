using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.K8s.Mappers.Abstract;

namespace TrivyOperator.Dashboard.Infrastructure.Persistence.Aggregators;

public class UidKeyedResourceAggregator<TKubernetesObject, TResource>(
    IResourceMapper<TKubernetesObject, TResource> mapper,
    IResourceKeyProvider<TKubernetesObject, Uid> keyProvider
) : ResourceAggregator<TKubernetesObject, TResource, Uid>(mapper, keyProvider)
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>
    where TResource : class, IEntity<Uid>;
