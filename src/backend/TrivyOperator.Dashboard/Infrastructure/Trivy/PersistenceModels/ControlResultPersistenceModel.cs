using MemoryPack;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.PersistenceModels;

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
