using TrivyOperator.Dashboard.Application.Home.Models;
using TrivyOperator.Dashboard.Application.Home.Queries.Models;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

namespace TrivyOperator.Dashboard.Application.Home.Services;

// the lists of names in the home page items: sorted and capped (the counts next to them are not)
public static class HomeNames
{
    public const int MaxNamesPerItem = 10;


    public static IReadOnlyList<string> ToNamespaceNames(
        IEnumerable<NamespaceName> namespaces,
        int limit = MaxNamesPerItem) =>
        [.. namespaces.Select(x => x.Value).Order(StringComparer.Ordinal).Take(limit),];

    public static IReadOnlyList<HomeBaseOsImageDto> ToBaseOsImages(
        IEnumerable<string> imageKeys,
        ActiveVulnerabilityTable table,
        int limit) =>
    [
        .. imageKeys
            .Select(key => table.Images.GetValueOrDefault(key))
            .OfType<ActiveImage>()
            .Select(image => new HomeBaseOsImageDto(
                ImageNameAndTag: $"{image.ImageMeta.Repo.Value}:{image.ImageMeta.Tag.Value}",
                ImageDigest: image.Digest?.Value ?? string.Empty,
                ImageRegistry: image.ImageMeta.Registry.Value))
            .Distinct()
            .OrderBy(x => x.ImageNameAndTag, StringComparer.Ordinal)
            .ThenBy(x => x.ImageDigest, StringComparer.Ordinal)
            .Take(limit),
    ];

    // the name an image has in the lists: repo:tag (the key itself for an image the table does not know)
    public static string ToImageName(string imageKey, ActiveVulnerabilityTable table) =>
        table.Images.TryGetValue(imageKey, out ActiveImage? image)
            ? $"{image.ImageMeta.Repo.Value}:{image.ImageMeta.Tag.Value}"
            : imageKey;

    public static IReadOnlyList<string> ToImageNames(
        IEnumerable<string> imageKeys,
        ActiveVulnerabilityTable table,
        int limit = MaxNamesPerItem) =>
    [
        .. imageKeys
            .Select(key => ToImageName(key, table))
            .Distinct()
            .Order(StringComparer.Ordinal)
            .Take(limit),
    ];
}
