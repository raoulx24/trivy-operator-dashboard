using TrivyOperator.Dashboard.Domain.ScanOnDemand.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.ScanOnDemand.Entities;

// the collected reports of one digest; only the selected types are filled in
// a later scan of the same digest replaces it
public sealed record OnDemandScanResult(
    Digest ImageDigest,
    ScanId LastScanId,
    Timestamp CollectedAt,
    Timestamp ExpiresAt,
    VulnerabilityReport? VulnerabilityReport,
    ExposedSecretReport? ExposedSecretReport,
    SbomReport? SbomReport);
