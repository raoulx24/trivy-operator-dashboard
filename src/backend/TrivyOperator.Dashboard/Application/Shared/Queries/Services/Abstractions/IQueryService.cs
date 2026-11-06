namespace TrivyOperator.Dashboard.Application.Shared.Queries.Services.Abstractions;

public interface IQueryService
{
    // false for the null services - the feature is disabled, so there is nothing to query
    bool IsEnabled { get; }
}
