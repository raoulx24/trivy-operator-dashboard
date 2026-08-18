using MemoryPack;

namespace TrivyOperator.Dashboard.Infrastructure.Persistence.Trivy.Models;

[MemoryPackable]
public sealed partial record ControlResultPersistenceModel(
    string Id,
    string ControlName,
    string Description,
    string Severity,
    string[] Checks,
    string[] Commands,
    int TotalFail
);
