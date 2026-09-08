using TrivyOperator.Dashboard.Domain.Kubernetes.Entities.Abstractions;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

namespace TrivyOperator.Dashboard.Domain.Kubernetes.Entities;

public sealed record K8sNamespace(Uid Id, NamespaceName NamespaceName, ResourceName Name, Timestamp LastSeenAt)
    : IK8sResource
{
    public bool HasNamespaceName(NamespaceName namespaceName) => NamespaceName == namespaceName;
}
