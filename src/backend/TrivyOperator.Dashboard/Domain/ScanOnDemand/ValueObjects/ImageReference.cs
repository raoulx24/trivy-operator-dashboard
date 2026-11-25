using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.ScanOnDemand.ValueObjects;

// a parsed, normalized image reference: [registry/]repository[:tag][@digest]
// defaults as the container runtimes apply them: docker.io registry, library/ for docker.io single-name
// repositories, latest when neither a tag nor a digest is given
public sealed partial record ImageReference
{
    public const string DefaultRegistry = "docker.io";
    private const string DefaultRegistryAlias = "index.docker.io";
    private const string DefaultTag = "latest";
    private const string OfficialImagesPrefix = "library/";
    private const int MaxLength = 1024;

    public ImageRegistry Registry { get; }
    public ImageRepository Repo { get; }
    // invalid when the reference was given by digest only
    public ImageTag Tag { get; }
    // invalid when the reference was given by tag only
    public Digest Digest { get; }

    // what goes into the pod spec
    public string FullName
    {
        get
        {
            string name = $"{Registry.Value}/{Repo.Value}";

            if (Tag.IsValid)
            {
                name += $":{Tag.Value}";
            }

            if (Digest.IsValid)
            {
                name += $"@{Digest.Value}";
            }

            return name;
        }
    }

    private ImageReference(ImageRegistry registry, ImageRepository repo, ImageTag tag, Digest digest)
    {
        Registry = registry;
        Repo = repo;
        Tag = tag;
        Digest = digest;
    }

    public static bool TryParse(
        string? input,
        [NotNullWhen(true)] out ImageReference? reference,
        [NotNullWhen(false)] out string? error)
    {
        reference = null;

        if (string.IsNullOrWhiteSpace(input))
        {
            error = "The image reference is empty.";
            return false;
        }

        string value = input.Trim();

        if (value.Length > MaxLength)
        {
            error = $"The image reference is longer than {MaxLength} characters.";
            return false;
        }

        if (value.Any(char.IsWhiteSpace))
        {
            error = "The image reference must not contain whitespace.";
            return false;
        }

        string? digest = null;
        int at = value.IndexOf('@');

        if (at >= 0)
        {
            digest = value[(at + 1)..];
            value = value[..at];

            if (!DigestRegex().IsMatch(digest))
            {
                error = $"'{digest}' is not a valid digest.";
                return false;
            }
        }

        // a ':' after the last '/' starts the tag; one before it belongs to the registry port
        string? tag = null;
        int colon = value.LastIndexOf(':');

        if (colon > value.LastIndexOf('/'))
        {
            tag = value[(colon + 1)..];
            value = value[..colon];

            if (!TagRegex().IsMatch(tag))
            {
                error = $"'{tag}' is not a valid tag.";
                return false;
            }
        }

        // the first component is a registry only when it looks like a host: a '.', a port or localhost
        string registry = DefaultRegistry;
        string repo = value;
        int firstSlash = value.IndexOf('/');

        if (firstSlash > 0)
        {
            string first = value[..firstSlash];

            if (first.Contains('.') || first.Contains(':') || first == "localhost")
            {
                registry = first.ToLowerInvariant();
                repo = value[(firstSlash + 1)..];
            }
        }

        if (!RegistryRegex().IsMatch(registry))
        {
            error = $"'{registry}' is not a valid registry.";
            return false;
        }

        // the repository is not lowercased: an uppercase one is rejected by the runtimes, so it is rejected here
        if (!RepositoryRegex().IsMatch(repo))
        {
            error = $"'{repo}' is not a valid repository (lowercase letters, digits and separators only).";
            return false;
        }

        if (registry == DefaultRegistryAlias)
        {
            registry = DefaultRegistry;
        }

        if (registry == DefaultRegistry && !repo.Contains('/'))
        {
            repo = OfficialImagesPrefix + repo;
        }

        if (tag is null && digest is null)
        {
            tag = DefaultTag;
        }

        reference = new ImageReference(new ImageRegistry(registry), new ImageRepository(repo), new ImageTag(tag), new Digest(digest));
        error = null;
        return true;
    }

    public override string ToString() => FullName;

    // host or IP, optional port
    [GeneratedRegex(@"^[a-z0-9](?:[a-z0-9-]*[a-z0-9])?(?:\.[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)*(?::[0-9]{1,5})?$")]
    private static partial Regex RegistryRegex();

    // OCI distribution spec path components separated by '/'
    [GeneratedRegex(@"^[a-z0-9]+(?:(?:[._]|__|-+)[a-z0-9]+)*(?:/[a-z0-9]+(?:(?:[._]|__|-+)[a-z0-9]+)*)*$")]
    private static partial Regex RepositoryRegex();

    [GeneratedRegex(@"^[A-Za-z0-9_][A-Za-z0-9_.-]{0,127}$")]
    private static partial Regex TagRegex();

    [GeneratedRegex(@"^[a-z0-9]+(?:[.+_-][a-z0-9]+)*:[a-f0-9]{32,}$", RegexOptions.IgnoreCase)]
    private static partial Regex DigestRegex();
}
