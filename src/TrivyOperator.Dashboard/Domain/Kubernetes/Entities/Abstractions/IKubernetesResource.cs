using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;

namespace TrivyOperator.Dashboard.Domain.Kubernetes.Entities.Abstractions;

public interface IKubernetesResource : IEntity<Uid>
{
    NamespaceName NamespaceName { get; }
    ResourceName Name { get; }
}
