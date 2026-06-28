using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Sboms;

public sealed record SbomComponent(
    ComponentId Id,
    ComponentName Name,
    ComponentVersion Version,
    Purl? Purl,
    ComponentType Type,
    Supplier? Supplier,
    IReadOnlyList<License> Licenses,
    IReadOnlyDictionary<string, string> Properties);