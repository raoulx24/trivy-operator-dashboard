using TrivyOperator.Dashboard.Domain.Kubernetes.Entities.Abstractions;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

namespace TrivyOperator.Dashboard.Domain.Kubernetes.Entities;

public sealed record KubernetesNamespace(Uid Id, NamespaceName NamespaceName, ResourceName Name, Timestamp LastSeenAt)
    : IKubernetesResource
{
    public bool HasNamespaceName(NamespaceName namespaceName) => NamespaceName == namespaceName;
}
