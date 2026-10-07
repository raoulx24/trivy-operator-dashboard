namespace TrivyOperator.Dashboard.Application.Shared.Queries.Models;

public class OperationResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public IDictionary<string, object>? Metadata { get; init; }
}
