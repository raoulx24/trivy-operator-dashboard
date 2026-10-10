using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

namespace TrivyOperator.Dashboard.Domain.Shared.Abstractions;

public interface IEntity<out TId> : IEntity
{
    TId Id { get; }
}

public interface IEntity
{
    Timestamp LastSeenAt { get; }

    bool HasNamespaceName(NamespaceName namespaceName);
}
