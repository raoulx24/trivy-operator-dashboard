using TrivyOperator.Dashboard.Domain.Shared.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Sboms;

// the distinct purls of the sbom of an image, sorted (ordinal), for the "where is this purl" lookups
// - the purls should be the canonical instances from the PurlDetail store, so the strings are not duplicated
// - a purl found several times in the image (e.g. on several paths) is kept once; invalid purls are skipped
// - equality compares the purls by content, so an unchanged image keeps its stored instance
public sealed record ImagePurls : IDetail<Digest>
{
    private static readonly Comparer<Purl> OrdinalComparer =
        Comparer<Purl>.Create(static (x, y) => string.CompareOrdinal(x.Value, y.Value));

    private readonly Purl[] purls;

    private ImagePurls(Digest id, Purl[] purls)
    {
        Id = id;
        this.purls = purls;
    }

    public Digest Id { get; }

    public IReadOnlyList<Purl> Purls => purls;

    public static ImagePurls Create(Digest id, IEnumerable<Purl> purls)
    {
        Purl[] sorted = [.. purls.Where(static x => x.IsValid).Distinct()];
        Array.Sort(sorted, OrdinalComparer);

        return new ImagePurls(id, sorted);
    }

    public bool Contains(Purl purl) => Array.BinarySearch(purls, purl, OrdinalComparer) >= 0;

    public bool Equals(ImagePurls? other)
        => other is not null &&
           Id == other.Id &&
           purls.AsSpan().SequenceEqual(other.purls);

    public override int GetHashCode() => Id.GetHashCode();
}
