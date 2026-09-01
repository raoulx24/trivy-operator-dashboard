using TrivyOperator.Dashboard.Application.Queries.Severities.Mappers;
using TrivyOperator.Dashboard.Application.Queries.Severities.Models;
using TrivyOperator.Dashboard.Application.Queries.Severities.Services.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Application.Queries.Severities.Services;

public class SeverityService : ISeverityService
{
    public Task<IReadOnlyList<SeverityDto>> GetAll(CancellationToken ctx = default)
    {
        IReadOnlyList<SeverityDto> result = [.. Severity.RankedSeverities.Select(static x => x.ToDto()),];

        return Task.FromResult(result);
    }
}
