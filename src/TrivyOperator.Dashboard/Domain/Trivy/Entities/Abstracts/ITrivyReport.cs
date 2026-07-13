using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

namespace TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;

public interface ITrivyReport
{
    Timestamp LastSeenAt { get; }
}
