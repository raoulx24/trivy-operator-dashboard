using MemoryPack;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.PersistenceModels;

[MemoryPackable]
public sealed partial record CheckPersistenceModel(
    string Category,
    string CheckId,
    string Description,
    string[] Messages,
    string Remediation,
    string Severity,
    bool Success,
    string Title
);
