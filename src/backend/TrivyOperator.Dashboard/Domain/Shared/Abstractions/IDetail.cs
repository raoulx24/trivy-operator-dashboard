namespace TrivyOperator.Dashboard.Domain.Shared.Abstractions;

// a piece of data referenced by id from entities (like a partner on an invoice), kept in its own store
public interface IDetail<out TId>
{
    TId Id { get; }
}
