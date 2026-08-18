using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Sboms;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Infrastructure.Persistence.Trivy.Models;

namespace TrivyOperator.Dashboard.Infrastructure.Persistence.Trivy.Mappers;

public static class ComponentPersistenceMapper
{
    public static ComponentPersistenceModel ToPersistenceModel(this Component domain)
    {
        return new ComponentPersistenceModel(
            domain.Id.Value,
            domain.Name.Value,
            domain.Version.Value,
            domain.Type.Value,
            domain.Purl?.Value,
            domain.Supplier?.Name,
            domain.Supplier?.Email,
            domain.Supplier?.Phone,
            domain.Licenses
                .Select(static x => new LicensePersistenceModel(
                    x.Id,
                    x.Name,
                    x.Url?.ToString()))
                .ToArray(),
            domain.Properties.ToDictionary(),
            domain.DependsOnIds
                .Select(static x => x.Value)
                .ToArray());
    }

    public static Component ToDomain(this ComponentPersistenceModel dto)
    {
        return new Component(
            new ComponentId(dto.Id),
            new ComponentName(dto.Name),
            new ComponentVersion(dto.Version),
            new ComponentType(dto.Type),
            dto.Purl is null ? null : new Purl(dto.Purl),
            dto.SupplierName is null && dto.SupplierPhone is null && dto.SupplierEmail is null 
                ? null
                : new Supplier(
                    dto.SupplierName,
                    dto.SupplierEmail,
                    dto.SupplierPhone),
            dto.Licenses
                .Select(static x => new License(
                    x.Id,
                    x.Name,
                    x.Url is null ? null : new Uri(x.Url)))
                .ToArray(),
            dto.Properties,
            [.. dto.DependsOnIds.Select(static x => new ComponentId(x))]);
    }
}
