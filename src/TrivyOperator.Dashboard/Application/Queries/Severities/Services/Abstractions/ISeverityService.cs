using TrivyOperator.Dashboard.Application.Queries.Severities.Models;

namespace TrivyOperator.Dashboard.Application.Queries.Severities.Services.Abstractions;

public interface ISeverityService
{
    Task<IReadOnlyList<SeverityDto>> GetAll(CancellationToken ctx = default);
}
