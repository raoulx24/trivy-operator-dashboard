using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Sboms;

namespace TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;

public interface ISbomReport<TSelf, out TId>
    : ITrivyReport<TSelf, TId>
where TSelf : ISbomReport<TSelf, TId>
{
    IReadOnlyList<Component> Components { get; }
    TSelf WithComponents(IReadOnlyList<Component> components);
}
