namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects;

public sealed record Package(
    PackageName Name,
    PackageVersion InstalledVersion,
    PackagePurl Purl,
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

public readonly record struct PackagePurl(string Value)
{
    public override string ToString() => Value;
}