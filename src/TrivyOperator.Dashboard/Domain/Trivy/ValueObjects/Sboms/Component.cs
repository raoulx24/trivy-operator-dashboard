using System.Collections.Immutable;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Sboms;

public sealed record Component(
    ComponentId Id,
    ComponentName Name,
    ComponentVersion Version,
    ComponentType Type,
    Purl? Purl,
    Supplier? Supplier,
    IReadOnlyList<License> Licenses,
    IReadOnlyDictionary<string, string> Properties,
    ImmutableArray<ComponentId> DependsOnIds);
    
public readonly record struct ComponentId
{
    private const string Sentinel = "n/a";

    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public ComponentId(string value)
    {
        Value = string.IsNullOrWhiteSpace(value) ? Sentinel : value.Trim();
    }

    public ComponentId() : this(Sentinel) { }

    public override string ToString() => Value;
}

public readonly record struct ComponentName
{
    private const string Sentinel = "n/a";

    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public ComponentName(string? value)
    {
        Value = string.IsNullOrWhiteSpace(value) ? Sentinel : value.Trim();
    }

    public ComponentName() : this(Sentinel) { }

    public override string ToString() => Value;
}

public readonly record struct ComponentVersion
{
    private const string Sentinel = "n/a";

    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public ComponentVersion(string? value)
    {
        Value = string.IsNullOrWhiteSpace(value) ? Sentinel : value.Trim();
    }

    public ComponentVersion() : this(Sentinel) { }

    public override string ToString() => Value;
}

public readonly record struct ComponentType
{
    private const string Sentinel = "n/a";

    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public ComponentType(string? value)
    {
        Value = string.IsNullOrWhiteSpace(value) ? Sentinel : value.Trim();
    }

    public ComponentType() : this(Sentinel) { }

    public override string ToString() => Value;
}
