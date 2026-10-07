namespace TrivyOperator.Dashboard.Application.Shared.Queries;

public sealed record QueryResponse<TResult>(TResult Payload, string? Error)
    where TResult: notnull;
