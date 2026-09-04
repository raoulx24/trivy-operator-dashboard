using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.K8s.Mappers.Abstract;

public interface IResourceMapper<in TKubernetesResource, TResource>
where TKubernetesResource : IKubernetesObject<V1ObjectMeta>
where TResource : IEntity
{
    TResource MapToDomain(TKubernetesResource cr, TResource? existing);
}
