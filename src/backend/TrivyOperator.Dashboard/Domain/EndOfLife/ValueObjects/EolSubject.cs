using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Vulnerabilities;

namespace TrivyOperator.Dashboard.Domain.EndOfLife.ValueObjects;

// the thing whose end of life is looked up, as seen on Trivy side
// for Kind = Os, Name is the Trivy OS family (e.g. alpine) and Version is the Trivy OS name (e.g. 3.18.4)
public readonly record struct EolSubject
{
    private const string Sentinel = "N/A";

    public EolSubjectKind Kind { get; }
    public string Name { get; }
    public string Version { get; }
    public bool IsValid => Name != Sentinel && Version != Sentinel;

    public EolSubject(EolSubjectKind kind, string? name, string? version)
    {
        Kind = kind;
        Name = string.IsNullOrWhiteSpace(name) || name.Trim() == Sentinel
            ? Sentinel
            : string.Intern(name.Trim().ToLowerInvariant());
        Version = string.IsNullOrWhiteSpace(version)
            ? Sentinel
            : string.Intern(version.Trim());
    }

    public EolSubject() : this(EolSubjectKind.Os, Sentinel, Sentinel) { }

    public static EolSubject FromOs(Os os) => new(EolSubjectKind.Os, os.Family.Value, os.Name.Value);

    public override string ToString() => $"{Kind}:{Name}:{Version}";
}

public enum EolSubjectKind
{
    Os,
}
