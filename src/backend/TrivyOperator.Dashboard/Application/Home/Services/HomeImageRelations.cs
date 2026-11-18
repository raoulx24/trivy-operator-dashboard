using TrivyOperator.Dashboard.Application.Home.Models;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using static TrivyOperator.Dashboard.Application.Home.Services.HomeNames;

namespace TrivyOperator.Dashboard.Application.Home.Services;

// the namespaces and the images of a reach as the lists of a detailed item: the images by name (the images of one name,
// with other digests, are one line), and per image the namespaces it runs in, as indexes in the namespaces
public sealed record HomeImageRelations(
    IReadOnlyList<string> NamespaceNames,
    IReadOnlyList<string> ImageNames,
    // image name -> its index in ImageNames
    IReadOnlyDictionary<string, int> ImageIndexes,
    IReadOnlyList<IReadOnlyList<int>> ImageNamespaceIndexes)
{
    public static HomeImageRelations Build(HomeReach reach, HomeScope scope, ActiveVulnerabilityTable table)
    {
        IReadOnlyList<string> namespaceNames = ToNamespaceNames(reach.Namespaces, int.MaxValue);
        IReadOnlyList<string> imageNames = ToImageNames(reach.ImageKeys, table, int.MaxValue);
        Dictionary<string, int> namespaceIndexes = ToIndexes(namespaceNames);
        Dictionary<string, int> imageIndexes = ToIndexes(imageNames);

        SortedSet<int>[] namespacesOfImage = [.. imageNames.Select(_ => new SortedSet<int>()),];
        foreach (string imageKey in reach.ImageKeys)
        {
            SortedSet<int> namespacesOfThis = namespacesOfImage[imageIndexes[ToImageName(imageKey, table)]];

            foreach (NamespaceName namespaceName in scope.GetReach(new[] { imageKey, }).Namespaces)
            {
                namespacesOfThis.Add(namespaceIndexes[namespaceName.Value]);
            }
        }

        return new HomeImageRelations(
            namespaceNames,
            imageNames,
            imageIndexes,
            [.. namespacesOfImage.Select(x => (IReadOnlyList<int>)x.ToArray()),]);
    }

    // the versions of the package installed in the images in scope, sorted
    public static IReadOnlyList<string> InstalledVersionsInScope(
        ActiveVulnerability row,
        HomeScope scope,
        ActiveVulnerabilityTable table) =>
    [
        .. row.ImageKeysByInstalledVersion
            .Where(x => x.Value.Any(key => scope.Contains(table.Images[key])))
            .Select(x => x.Key)
            .Order(StringComparer.Ordinal),
    ];

    // name -> its index in the list (the names are distinct)
    private static Dictionary<string, int> ToIndexes(IReadOnlyList<string> names) =>
        names.Select((name, index) => (name, index)).ToDictionary(x => x.name, x => x.index);
}
