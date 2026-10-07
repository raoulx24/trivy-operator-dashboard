using TrivyOperator.Dashboard.Application.Trivy.Queries.Severities.Models;

namespace TrivyOperator.Dashboard.Application.Trivy.Queries.Severities.Services.Abstractions;

public interface ISeverityService
{
    Task<IReadOnlyList<SeverityDto>> GetAll(CancellationToken ctx = default);
}
