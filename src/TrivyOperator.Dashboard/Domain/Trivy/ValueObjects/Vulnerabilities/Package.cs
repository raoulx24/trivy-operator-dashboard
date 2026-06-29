using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Vulnerabilities;

public sealed record Package(
    PackageName Name,
    PackageVersion InstalledVersion,
    Purl Purl,
    Target Target
);

public readonly record struct PackageName
{
    private const string Sentinel = "N/A";

    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public PackageName(string value)
    {
        Value = string.IsNullOrWhiteSpace(value) ? Sentinel : value.Trim();
    }

    public PackageName() : this(Sentinel) { }

    public override string ToString() => Value;
}

public readonly record struct PackageVersion
{
    private const string Sentinel = "N/A";

    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public PackageVersion(string value)
    {
        Value = string.IsNullOrWhiteSpace(value) ? Sentinel : value.Trim();
    }

    public PackageVersion() : this(Sentinel) { }

    public override string ToString() => Value;
}
