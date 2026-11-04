using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.SecurityAssessments;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using CheckPersistenceModel = TrivyOperator.Dashboard.Infrastructure.Trivy.PersistenceModels.CheckPersistenceModel;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.ToPersistence;

public static class CheckPersistenceMapper
{
    // the persistence model keeps its shape; Messages and Success are no longer part of the domain
    public static CheckPersistenceModel ToPersistenceModel(this Check domain)
    {
        return new CheckPersistenceModel(
            domain.Category.Value,
            domain.CheckId.Value,
            domain.Description.Value,
            [],
            domain.Remediation.Value,
            domain.Severity.Value,
            false,
            domain.Title.Value);
    }

    public static Check ToDomain(this CheckPersistenceModel dto)
    {
        return new(
            new Category(dto.Category),
            new CheckId(dto.CheckId),
            new Description(dto.Description),
            new Remediation(dto.Remediation),
            new Severity(dto.Severity),
            new Title(dto.Title));
    }
}
