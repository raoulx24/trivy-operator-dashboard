using TrivyOperator.Dashboard.Application.Shared.Queries.Services.Abstractions;

namespace TrivyOperator.Dashboard.Application.Shared.Queries.Services;

// base class for the null query services
public abstract class QueryNullService : IQueryService
{
    public bool IsEnabled => false;
}
