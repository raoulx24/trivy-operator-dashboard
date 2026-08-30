namespace TrivyOperator.Dashboard.Application.Queries.Shared;

public sealed record QueryResponse<TResult>(TResult Result, string? Error)
    where TResult: notnull;
