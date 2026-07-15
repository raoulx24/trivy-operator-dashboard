using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;

namespace TrivyOperator.Dashboard.Domain.K8s.Entities.Abstractions;

public interface IK8sResource : IEntity<Uid>
{
    NamespaceName NamespaceName { get; }
    ResourceName Name { get; }
}
