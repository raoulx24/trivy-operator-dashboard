using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

namespace TrivyOperator.Dashboard.Application.Kubernetes.Namespaces;

// how the namespaced report watchers are laid out
// - ClusterWide: no namespace list configured; one watcher per type for all namespaces (needs cluster wide RBAC)
// - PerNamespace: a namespace list is configured; one watcher per type and namespace
public enum WatchMode
{
    ClusterWide,
    PerNamespace,
}

// which namespaces the main pipeline accepts; fixed at startup
public sealed class NamespacePolicy
{
    public NamespacePolicy(IReadOnlySet<NamespaceName>? included, IReadOnlySet<NamespaceName> excluded)
    {
        Included = included;
        Excluded = excluded;

        WatchedNamespaces = included is null
            ? []
            : [.. included.Where(x => !excluded.Contains(x)).OrderBy(x => x.Value, StringComparer.Ordinal),];

        ExcludedFieldSelector = excluded.Count == 0
            ? null
            : string.Join(
                ",",
                excluded.OrderBy(x => x.Value, StringComparer.Ordinal).Select(x => $"metadata.namespace!={x.Value}"));
    }

    // null = all namespaces (minus the excluded ones)
    public IReadOnlySet<NamespaceName>? Included { get; }

    // configured exclusions plus reserved namespaces (internal features); exclusion wins over inclusion
    public IReadOnlySet<NamespaceName> Excluded { get; }

    public WatchMode Mode => Included is null ? WatchMode.ClusterWide : WatchMode.PerNamespace;

    // PerNamespace: the namespaces that get watchers (included minus excluded); ClusterWide: empty
    public IReadOnlyList<NamespaceName> WatchedNamespaces { get; }

    // "metadata.namespace!=a,metadata.namespace!=b" for cluster wide lists and watches; null when nothing is excluded
    public string? ExcludedFieldSelector { get; }

    // cluster scoped resources (no namespace) are always allowed
    public bool IsAllowed(NamespaceName namespaceName) =>
        namespaceName.IsClusterScoped
        || (!Excluded.Contains(namespaceName) && (Included is null || Included.Contains(namespaceName)));
}
