using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

namespace TrivyOperator.Dashboard.Application.Home.Models;

// the part of the table a query looks at: all of it, or only what runs in some namespaces
// with namespaces, the cluster scoped images and reports are out, and the reach of a row counts only the images,
// namespaces and workloads inside them
public sealed class HomeScope(ActiveVulnerabilityTable table, IReadOnlyCollection<string>? namespaces)
{
    private readonly HashSet<NamespaceName>? namespaceNames =
        namespaces is { Count: > 0, } ? [.. namespaces.Select(x => new NamespaceName(x)),] : null;

    public bool Contains(ActiveImage image) =>
        namespaceNames is null || image.WorkloadsByNamespace.Keys.Any(namespaceNames.Contains);

    public bool Contains(NamespaceName namespaceName, bool isClusterScoped) =>
        namespaceNames is null || (!isClusterScoped && namespaceNames.Contains(namespaceName));

    public bool Contains(ActiveVulnerability row) => row.ImageKeys.Any(key => Contains(table.Images[key]));

    public HomeReach GetReach(ActiveVulnerability row) => GetReach(row.ImageKeys);

    public HomeReach GetReach(IEnumerable<string> imageKeys)
    {
        HashSet<string> images = [];
        HashSet<NamespaceName> reachedNamespaces = [];
        HashSet<Uid> workloads = [];

        foreach (string key in imageKeys)
        {
            ActiveImage image = table.Images[key];

            if (!Contains(image) || !images.Add(key))
            {
                continue;
            }

            workloads.UnionWith(image.ClusterWorkloads);

            foreach ((NamespaceName namespaceName, HashSet<Uid> namespaceWorkloads) in image.WorkloadsByNamespace)
            {
                if (namespaceNames is null || namespaceNames.Contains(namespaceName))
                {
                    reachedNamespaces.Add(namespaceName);
                    workloads.UnionWith(namespaceWorkloads);
                }
            }
        }

        return new HomeReach(images, reachedNamespaces, workloads);
    }
}

public sealed record HomeReach(
    IReadOnlySet<string> ImageKeys,
    IReadOnlySet<NamespaceName> Namespaces,
    IReadOnlySet<Uid> Workloads);
