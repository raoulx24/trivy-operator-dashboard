using System.Collections.Concurrent;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Sboms;

// a purl found in the sbom reports, shared by all the images that contain it (see ImagePurls)
// - the stored instance is the canonical one: the images keep its Purl, so the purl string is held once
// - the licenses are the ones of the newest report that had the purl
// - equality compares the licenses by content, so an unchanged purl keeps its stored (canonical) instance
public sealed record PurlDetail : IDetail<Purl>
{
    // the licenses are few and repeat across purls: the instances and the lists are shared, never removed
    private static readonly ConcurrentDictionary<License, License> LicensePool = new();
    private static readonly ConcurrentDictionary<string, License[]> LicenseListPool = new(StringComparer.Ordinal);

    private PurlDetail(Purl id, License[] licenses)
    {
        Id = id;
        Licenses = licenses;
    }

    public Purl Id { get; }

    public IReadOnlyList<License> Licenses { get; }

    // licenses: by id, else by name (no url); without both they are skipped; duplicates are removed, the order is
    // ordinal (id first, then name), so the same set gives the same shared list
    public static PurlDetail Create(Purl id, IEnumerable<License> licenses)
    {
        License[] slim =
        [
            .. licenses
                .Select(static x => x.Id is not null
                    ? new License(x.Id, null, null)
                    : x.Name is not null ? new License(null, x.Name, null) : null)
                .OfType<License>()
                .Distinct()
                .OrderBy(static x => x.Id is null)
                .ThenBy(static x => x.Id ?? x.Name, StringComparer.Ordinal),
        ];

        if (slim.Length == 0)
        {
            return new PurlDetail(id, []);
        }

        string key = string.Join('\n', slim.Select(static x => x.Id is not null ? $"i:{x.Id}" : $"n:{x.Name}"));

        License[] shared = LicenseListPool.GetOrAdd(
            key,
            static (_, list) => [.. list.Select(static x => LicensePool.GetOrAdd(x, x))],
            slim);

        return new PurlDetail(id, shared);
    }

    public bool Equals(PurlDetail? other)
        => other is not null &&
           Id == other.Id &&
           Licenses.SequenceEqual(other.Licenses);

    public override int GetHashCode() => Id.GetHashCode();
}
