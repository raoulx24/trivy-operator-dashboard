using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ExposedSecrets;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Infrastructure.Persistence.Trivy.Entities;

namespace TrivyOperator.Dashboard.Infrastructure.Persistence.Trivy.Mappers;

public static class SecretPersistenceMapper
{
    public static SecretPersistenceModel ToPersistenceModel(this Secret domain)
    {
        return new SecretPersistenceModel(
            domain.Rule.Category.Value,
            domain.Rule.RuleId.Value,
            domain.Rule.Severity.Value,
            domain.Rule.Title.Value,
            domain.Match.Value,
            domain.Target.Value);
    }

    public static Secret ToDomain(this SecretPersistenceModel dto)
    {
        return new Secret(
            new Rule(
                new Category(dto.Category),
                new RuleId(dto.RuleId),
                new Severity(dto.Severity),
                new Title(dto.RuleTitle)),
            new Match(dto.Match),
            new Target(dto.Target));
    }
}
