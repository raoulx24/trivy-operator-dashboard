using MemoryPack;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.PersistenceModels;

[MemoryPackable]
public sealed partial record ComponentPersistenceModel(
    string Name,
    string Version,
    string Type,
    string? Purl,
    string? SupplierName,
    string? SupplierEmail,
    string? SupplierPhone,
    LicensePersistenceModel[] Licenses,
    ComponentPropertyPersistenceModel[] Properties,
    // positions in the persisted component array
    int[] DependsOnIds,
    int? LayerIndex
);

[MemoryPackable]
public sealed partial record ComponentPropertyPersistenceModel(
    string Name,
    string Value
);

[MemoryPackable]
public sealed partial record LicensePersistenceModel(
    string? Id,
    string? Name,
    string? Url
);
