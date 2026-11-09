using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Vulnerabilities;

namespace TrivyOperator.Dashboard.Domain.History.FixedVersionHistory.Entities;

// what is needed to decide on an incoming value: the last entry and when an event last saw the pair
public sealed record FixedVersionState(
    PackageVulnerabilityKey Key,
    FixedVersionEntry Latest,
    Timestamp TouchedAt
);
