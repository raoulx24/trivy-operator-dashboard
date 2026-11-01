namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

public readonly record struct Purl
{
    private const string Sentinel = "N/A";
    private const string Scheme = "pkg:";

    private static readonly IReadOnlyDictionary<string, string> EmptyQualifiers =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public string Value { get => field ?? Sentinel; }
    public bool IsValid => Value != Sentinel;

    public Purl(string? value)
    {
        Value = string.IsNullOrWhiteSpace(value) ? Sentinel : value.Trim();
    }

    public Purl() : this(Sentinel) { }

    // Components are parsed on demand (not stored) to keep the struct a single string reference.
    // Format: pkg:type/namespace/name@version?qualifiers#subpath (https://github.com/package-url/purl-spec)

    /// <summary>Package type / ecosystem, lowercased (e.g. "deb", "apk", "nuget", "golang").</summary>
    public string? Type => Parse(Value).Type;

    /// <summary>Namespace, e.g. "ubuntu", "alpine"; null when absent (e.g. nuget, golang/stdlib).</summary>
    public string? Namespace => Parse(Value).Namespace;

    public string? Name => Parse(Value).Name;

    public string? Version => Parse(Value).Version;

    /// <summary>Qualifiers after '?', keys case-insensitive; empty when none.</summary>
    public IReadOnlyDictionary<string, string> Qualifiers => Parse(Value).Qualifiers;

    public string? Subpath => Parse(Value).Subpath;

    public string? Distro => Qualifiers.GetValueOrDefault("distro");

    public string? Arch => Qualifiers.GetValueOrDefault("arch");

    /// <summary>Parses all components in a single pass; prefer it when reading more than one component.</summary>
    public Components Parse() => Parse(Value);

    private static Components Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value == Sentinel ||
            !value.StartsWith(Scheme, StringComparison.OrdinalIgnoreCase))
        {
            return Components.Empty;
        }

        string remainder = value[Scheme.Length..];

        string? subpath = null;
        int hashIndex = remainder.LastIndexOf('#');
        if (hashIndex >= 0)
        {
            subpath = NullIfEmpty(Decode(remainder[(hashIndex + 1)..].Trim('/')));
            remainder = remainder[..hashIndex];
        }

        IReadOnlyDictionary<string, string> qualifiers = EmptyQualifiers;
        int questionIndex = remainder.LastIndexOf('?');
        if (questionIndex >= 0)
        {
            qualifiers = ParseQualifiers(remainder[(questionIndex + 1)..]);
            remainder = remainder[..questionIndex];
        }

        remainder = remainder.Trim('/');

        int typeEnd = remainder.IndexOf('/');
        if (typeEnd <= 0)
        {
            return Components.Empty with { Qualifiers = qualifiers, Subpath = subpath };
        }

        string type = remainder[..typeEnd].ToLowerInvariant();
        remainder = remainder[(typeEnd + 1)..];

        string? version = null;
        int atIndex = remainder.LastIndexOf('@');
        if (atIndex >= 0)
        {
            version = NullIfEmpty(Decode(remainder[(atIndex + 1)..]));
            remainder = remainder[..atIndex];
        }

        int nameStart = remainder.LastIndexOf('/');
        string? name = NullIfEmpty(Decode(remainder[(nameStart + 1)..]));
        string? @namespace = nameStart > 0
            ? NullIfEmpty(string.Join('/', remainder[..nameStart].Split('/', StringSplitOptions.RemoveEmptyEntries).Select(Decode)))
            : null;

        return new Components(type, @namespace, name, version, qualifiers, subpath);
    }

    private static IReadOnlyDictionary<string, string> ParseQualifiers(string qualifiers)
    {
        Dictionary<string, string> result = new(StringComparer.OrdinalIgnoreCase);
        foreach (string pair in qualifiers.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            int equalsIndex = pair.IndexOf('=');
            if (equalsIndex <= 0)
            {
                continue;
            }

            string key = pair[..equalsIndex].ToLowerInvariant();
            string val = Decode(pair[(equalsIndex + 1)..]);
            if (val.Length > 0)
            {
                result[key] = val;
            }
        }

        return result.Count == 0 ? EmptyQualifiers : result;
    }

    private static string Decode(string value) => Uri.UnescapeDataString(value);

    private static string? NullIfEmpty(string value) => value.Length == 0 ? null : value;

    public readonly record struct Components(
        string? Type,
        string? Namespace,
        string? Name,
        string? Version,
        IReadOnlyDictionary<string, string> Qualifiers,
        string? Subpath)
    {
        internal static readonly Components Empty = new(null, null, null, null, EmptyQualifiers, null);

        public string? Distro => Qualifiers.GetValueOrDefault("distro");

        public string? Arch => Qualifiers.GetValueOrDefault("arch");
    }

    public override string ToString() => Value;
}
