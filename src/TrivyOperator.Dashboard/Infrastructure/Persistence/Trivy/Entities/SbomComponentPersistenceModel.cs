using MemoryPack;

namespace TrivyOperator.Dashboard.Infrastructure.Persistence.Trivy.Entities;

[MemoryPackable]
public sealed partial record ComponentPersistenceModel(
    string Id,
    string Name,
    string Version,
    string Type,
    string? Purl,
    string? SupplierName,
    string? SupplierEmail,
    string? SupplierPhone,
    LicensePersistenceModel[] Licenses,
    Dictionary<string, string> Properties,
    string[] DependsOnIds
);

[MemoryPackable]
public sealed partial record LicensePersistenceModel(
    string? Id,
    string? Name,
    string? Url
);
