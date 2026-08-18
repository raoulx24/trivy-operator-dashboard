using MemoryPack;

namespace TrivyOperator.Dashboard.Infrastructure.Persistence.Trivy.Models;

[MemoryPackable]
public sealed partial record SecretPersistenceModel(
    string Category,
    string RuleId,
    string Severity,
    string RuleTitle,
    string Match,
    string Target
);
