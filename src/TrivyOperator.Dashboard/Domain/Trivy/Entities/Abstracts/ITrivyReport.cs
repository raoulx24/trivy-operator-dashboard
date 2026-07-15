using TrivyOperator.Dashboard.Domain.Shared.Abstractions;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

namespace TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;

public interface ITrivyReport<out TId> : IEntity<TId>, ITrivyReport
{
}

public interface ITrivyReport
{
    Timestamp LastSeenAt { get; }
}
