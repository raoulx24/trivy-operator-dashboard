namespace TrivyOperator.Dashboard.Application.Queries.Shared;

public sealed record QueryResponse<TResult>(TResult Payload, string? Error)
    where TResult: notnull;
