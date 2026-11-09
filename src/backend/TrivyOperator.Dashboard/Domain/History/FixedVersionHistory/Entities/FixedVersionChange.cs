using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Vulnerabilities;

namespace TrivyOperator.Dashboard.Domain.History.FixedVersionHistory.Entities;

// a change of the fixed version of a pair (the baseline, the first entry, is not a change)
public sealed record FixedVersionChange(PackageVulnerabilityKey Key, Timestamp At);
