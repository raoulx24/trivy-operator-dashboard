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

    public static PackageKey From(Package package)
    {
        Purl.Components purl = package.Purl.Parse();

        if (purl.Type is null || purl.Name is null)
        {
            return new PackageKey($"{package.Name.Value}|{package.Target.Value}");
        }

        string @namespace = purl.Namespace is null ? string.Empty : $"{purl.Namespace}/";
        string distro = purl.Distro is null ? string.Empty : $"?distro={purl.Distro}";

        return new PackageKey($"pkg:{purl.Type}/{@namespace}{purl.Name}{distro}");
    }

    public override string ToString() => Value;
}
