using TrivyOperator.Dashboard.Domain.K8s.Entities.Abstractions;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

namespace TrivyOperator.Dashboard.Domain.K8s.Entities;

public sealed record K8sNamespace(Uid Id, NamespaceName NamespaceName, ResourceName Name, Timestamp LastSeenAt)
    : IK8sResource
{
    public bool HasNamespaceName(NamespaceName namespaceName) => NamespaceName == namespaceName;
}
