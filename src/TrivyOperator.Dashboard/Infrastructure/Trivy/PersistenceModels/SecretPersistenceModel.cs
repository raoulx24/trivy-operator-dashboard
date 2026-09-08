using MemoryPack;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.PersistenceModels;

[MemoryPackable]
public sealed partial record SecretPersistenceModel(
    string Category,
    string RuleId,
    string Severity,
    string RuleTitle,
    string Match,
    string Target
);
