using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

namespace TrivyOperator.Dashboard.Domain.Shared.Abstractions;

public interface IEntity<out TId>
{
    TId Id { get; }
    Timestamp LastSeenAt { get; }

    bool HasNamespaceName(NamespaceName namespaceName);
}
