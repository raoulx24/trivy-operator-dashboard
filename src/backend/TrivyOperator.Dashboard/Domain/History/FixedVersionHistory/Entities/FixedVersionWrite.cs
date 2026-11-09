using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Vulnerabilities;

namespace TrivyOperator.Dashboard.Domain.History.FixedVersionHistory.Entities;

public enum FixedVersionWriteKind
{
    // first time the pair is seen: the entry is the baseline
    Baseline,
    // the fixed version differs from the last entry, in a newer report: the entry is a change
    Change,
    // nothing to record, only the touch (the pair is still in use)
    Touch,
}

// what the recorder decided for one pair; Entry is null for Touch
public sealed record FixedVersionWrite(
    FixedVersionWriteKind Kind,
    PackageVulnerabilityKey Key,
    FixedVersionEntry? Entry,
    Timestamp TouchedAt
);
