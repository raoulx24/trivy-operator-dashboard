using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.SecurityAssessments;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Infrastructure.Persistence.Trivy.Models;

namespace TrivyOperator.Dashboard.Infrastructure.Persistence.Trivy.Mappers;

public static class CheckPersistenceMapper
{
    public static CheckPersistenceModel ToPersistenceModel(this Check domain)
    {
        return new CheckPersistenceModel(
            domain.Category.Value,
            domain.CheckId.Value,
            domain.Description.Value,
            domain.Messages.ToArray(),
            domain.Remediation.Value,
            domain.Severity.Value,
            domain.Success,
            domain.Title.Value);
    }

    public static Check ToDomain(this CheckPersistenceModel dto)
    {
        return new(
            new Category(dto.Category),
            new CheckId(dto.CheckId),
            new Description(dto.Description),
            dto.Messages,
            new Remediation(dto.Remediation),
            new Severity(dto.Severity),
            dto.Success,
            new Title(dto.Title));
    }
}
