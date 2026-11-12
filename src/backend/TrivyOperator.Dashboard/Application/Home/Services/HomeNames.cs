using TrivyOperator.Dashboard.Application.Home.Models;
using TrivyOperator.Dashboard.Application.Home.Queries.Models;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

namespace TrivyOperator.Dashboard.Application.Home.Services;

// the lists of names in the home page items: sorted and capped (the counts next to them are not)
public static class HomeNames
{
    public const int MaxNamesPerItem = 10;


    public static IReadOnlyList<string> ToNamespaceNames(IEnumerable<NamespaceName> namespaces) =>
        [.. namespaces.Select(x => x.Value).Order(StringComparer.Ordinal).Take(MaxNamesPerItem),];

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

    public static IReadOnlyList<string> ToImageNames(IEnumerable<string> imageKeys, ActiveVulnerabilityTable table) =>
    [
        .. imageKeys
            .Select(key => table.Images.TryGetValue(key, out ActiveImage? image)
                ? $"{image.ImageMeta.Repo.Value}:{image.ImageMeta.Tag.Value}"
                : key)
            .Distinct()
            .Order(StringComparer.Ordinal)
            .Take(MaxNamesPerItem),
    ];
}
