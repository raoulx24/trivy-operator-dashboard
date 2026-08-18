using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ClusterCompliance;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Infrastructure.Persistence.Trivy.Models;

namespace TrivyOperator.Dashboard.Infrastructure.Persistence.Trivy.Mappers;

public static class ControlResultPersistenceMapper
{
    public static ControlResultPersistenceModel ToPersistenceModel(this ControlResult domain)
    {
        return new ControlResultPersistenceModel(
            domain.Control.Id.Value,
            domain.Control.ControlName.Value,
            domain.Control.Description.Value,
            domain.Control.Severity.Value,
            domain.Control.Checks
                .Select(static x => x.Value)
                .ToArray(),
            domain.Control.Commands
                .Select(static x => x.Value)
                .ToArray(),
            domain.TotalFail.Value);
    }

    public static ControlResult ToDomain(this ControlResultPersistenceModel dto)
    {
        return new ControlResult(
            new Control(
                new ControlId(dto.Id),
                new ControlName(dto.ControlName),
                new ControlDescription(dto.Description),
                new Severity(dto.Severity),
                dto.Checks
                    .Select(static x => new ControlCheckId(x))
                    .ToArray(),
                dto.Commands
                    .Select(static x => new ControlCommandId(x))
                    .ToArray()),
            new CheckResultTotalFail(dto.TotalFail));
    }
}
