using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Sboms;

namespace TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;

public interface ISbomReport<out TSelf, out TId>
    : ITrivyReport<TId>
{
    IReadOnlyList<Component> Components { get; }
    TSelf WithComponents(IReadOnlyList<Component> components);
}
