using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Vulnerabilities;

// a package regardless of its installed version: the scope of the vulnerability data that depends on the package
// (severity, score, fixed version)
// - from the purl: type/namespace/name plus the distro qualifier (the advisories are per distro release)
// - a package without a usable purl falls back to its name and target
public readonly record struct PackageKey
{
    private const string Sentinel = "N/A";

    public string Value { get => field ?? Sentinel; }
    public bool IsValid => Value != Sentinel;

    public PackageKey(string? value)
    {
        Value = string.IsNullOrWhiteSpace(value) ? Sentinel : string.Intern(value.Trim());
    }

    public PackageKey() : this(Sentinel) { }

    public static PackageKey From(Package package) =>
        FromPurl(package.Purl) ?? new PackageKey($"{package.Name.Value}|{package.Target.Value}");

    // null when the purl has no type or name
    public static PackageKey? FromPurl(Purl purl)
    {
        Purl.Components components = purl.Parse();

        if (components.Type is null || components.Name is null)
        {
            return null;
        }

        string @namespace = components.Namespace is null ? string.Empty : $"{components.Namespace}/";
        string distro = components.Distro is null ? string.Empty : $"?distro={components.Distro}";

        return new PackageKey($"pkg:{components.Type}/{@namespace}{components.Name}{distro}");
    }

    public override string ToString() => Value;
}
