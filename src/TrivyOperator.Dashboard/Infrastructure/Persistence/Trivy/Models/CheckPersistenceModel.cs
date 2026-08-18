using MemoryPack;

namespace TrivyOperator.Dashboard.Infrastructure.Persistence.Trivy.Models;

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
