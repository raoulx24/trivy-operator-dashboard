using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Sboms;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using ComponentPersistenceModel = TrivyOperator.Dashboard.Infrastructure.Trivy.PersistenceModels.ComponentPersistenceModel;
using LicensePersistenceModel = TrivyOperator.Dashboard.Infrastructure.Trivy.PersistenceModels.LicensePersistenceModel;
using ComponentPropertyPersistenceModel = TrivyOperator.Dashboard.Infrastructure.Trivy.PersistenceModels.ComponentPropertyPersistenceModel;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.ToPersistence;

// Component ids are not persisted: Trivy's bom-refs are random per scan and do not compress.
// A component is identified by its position, dependencies are stored as positions,
// and on decode every component gets an id derived from the report key and its position.
public static class ComponentPersistenceMapper
{
    public static ComponentPersistenceModel ToPersistenceModel(
        this Component domain,
        IReadOnlyDictionary<ComponentId, int> indexById)
    {
        return new ComponentPersistenceModel(
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
            domain.Properties
                .Select(static x => new ComponentPropertyPersistenceModel(x.Name, x.Value))
                .ToArray(),
            domain.DependsOnIds
                .Select(x => indexById.TryGetValue(x, out int index) ? index : -1)
                .Where(static x => x >= 0)
                .ToArray(),
            domain.LayerIndex);
    }

    public static Component ToDomain(this ComponentPersistenceModel dto, ComponentId id, ComponentId[] ids)
    {
        return new Component(
            id,
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
            dto.Properties
                .Select(static x => new ComponentProperty(x.Name, x.Value))
                .ToArray(),
            [.. dto.DependsOnIds.Where(x => x >= 0 && x < ids.Length).Select(x => ids[x])],
            dto.LayerIndex);
    }

    // First occurrence wins when Trivy repeats a bom-ref.
    public static Dictionary<ComponentId, int> ToIndexById(IReadOnlyList<Component> components)
    {
        Dictionary<ComponentId, int> indexById = new(components.Count);
        for (int i = 0; i < components.Count; i++)
        {
            indexById.TryAdd(components[i].Id, i);
        }

        return indexById;
    }

    // Stable for a report key and position: the report key's hash with the position in the last 4 bytes.
    // Hashes once per report, so decoding stays cheap.
    public static ComponentId[] CreateComponentIds(string reportKey, int count)
    {
        Span<byte> hash = stackalloc byte[32];
        SHA256.HashData(Encoding.UTF8.GetBytes(reportKey), hash);
        Span<byte> guidBytes = hash[..16];

        ComponentId[] ids = new ComponentId[count];
        for (int i = 0; i < count; i++)
        {
            BinaryPrimitives.WriteInt32LittleEndian(guidBytes[12..], i);
            ids[i] = new ComponentId(new Guid(guidBytes).ToString());
        }

        return ids;
    }
}
