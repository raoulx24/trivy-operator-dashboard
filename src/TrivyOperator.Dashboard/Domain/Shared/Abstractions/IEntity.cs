namespace TrivyOperator.Dashboard.Domain.Shared.Abstractions;

public interface IEntity<out TId>
{
    TId Id { get; }
}
