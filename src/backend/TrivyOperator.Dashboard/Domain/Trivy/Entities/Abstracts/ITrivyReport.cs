using TrivyOperator.Dashboard.Domain.Shared.Abstractions;

namespace TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;

public interface ITrivyReport<out TId> : IEntity<TId>, ITrivyReport
{
}

public interface ITrivyReport : IEntity
{
}
