using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Vulnerabilities;

public sealed record Package(
    PackageName Name,
    PackageVersion InstalledVersion,
    Purl Purl,
    string Target
);

public readonly record struct PackageName(string Value)
{
    public override string ToString() => Value;
}

public readonly record struct PackageVersion(string Value)
{
    public override string ToString() => Value;
}
