using TrivyOperator.Dashboard.Application.Shared.Queries.Services.Abstractions;

namespace TrivyOperator.Dashboard.Application.Shared.Queries.Services;

// base class for the real query services
public abstract class QueryService : IQueryService
{
    public bool IsEnabled => true;
}
