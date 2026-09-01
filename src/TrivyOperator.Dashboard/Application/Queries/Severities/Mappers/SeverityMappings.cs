using TrivyOperator.Dashboard.Application.Queries.Severities.Models;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Application.Queries.Severities.Mappers;

public static class SeverityMappings
{
    public static SeverityDto ToDto(this Severity severity)
    {
        return new SeverityDto(
            Id: severity.Rank,
            Name: severity.Value
        );
    }
}
