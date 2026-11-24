using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using TrivyOperator.Dashboard.Application.Kubernetes.Namespaces.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.Options;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

namespace TrivyOperator.Dashboard.Application.Kubernetes.Namespaces;

// builds the policy from Kubernetes:NamespaceList, Kubernetes:ExcludedNamespaceList and the reserved namespaces
// invalid configuration fails at startup (the first Get), with a message naming the setting
public sealed partial class NamespacePolicyProvider(
    IOptions<KubernetesOptions> options,
    ILogger<NamespacePolicyProvider> logger
) : INamespacePolicyProvider
{
    private const int MaxNamespaceNameLength = 63;

    private readonly Lock gate = new();
    private readonly HashSet<NamespaceName> reserved = [];
    private NamespacePolicy? policy;

    public NamespacePolicy Get(ContextName contextName)
    {
        lock (gate)
        {
            return policy ??= Build();
        }
    }

    public void Reserve(NamespaceName namespaceName)
    {
        lock (gate)
        {
            if (policy is not null)
            {
                throw new InvalidOperationException(
                    $"Namespace {namespaceName} cannot be reserved after the namespace policy is fixed.");
            }

            reserved.Add(namespaceName);
        }
    }

    private NamespacePolicy Build()
    {
        HashSet<NamespaceName>? included = Parse(options.Value.NamespaceList, "Kubernetes:NamespaceList");
        HashSet<NamespaceName> excluded = Parse(options.Value.ExcludedNamespaceList, "Kubernetes:ExcludedNamespaceList") ?? [];

        excluded.UnionWith(reserved);

        if (included is not null)
        {
            NamespaceName[] both = [.. included.Where(excluded.Contains),];

            if (both.Length > 0)
            {
                logger.LogWarning(
                    "Namespaces {namespaces} are both included and excluded; they are excluded.",
                    string.Join(", ", both.Select(x => x.Value)));
            }
        }

        NamespacePolicy namespacePolicy = new(included, excluded);

        if (namespacePolicy.Mode == WatchMode.PerNamespace && namespacePolicy.WatchedNamespaces.Count == 0)
        {
            throw new InvalidOperationException(
                "Kubernetes:NamespaceList leaves no namespace to watch once Kubernetes:ExcludedNamespaceList "
                + "and the reserved namespaces are removed.");
        }

        logger.LogInformation(
            "Namespace policy - mode {mode} - watched {watched} - excluded {excluded}",
            namespacePolicy.Mode,
            namespacePolicy.Mode == WatchMode.ClusterWide
                ? "all"
                : string.Join(", ", namespacePolicy.WatchedNamespaces.Select(x => x.Value)),
            excluded.Count == 0 ? "none" : string.Join(", ", excluded.Select(x => x.Value)));

        return namespacePolicy;
    }

    // null when the setting is empty; throws on names that are not valid namespace names
    private static HashSet<NamespaceName>? Parse(string? value, string settingName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        HashSet<NamespaceName> namespaces = [];

        foreach (string part in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            NamespaceName namespaceName = new(part);

            if (namespaceName.Value.Length > MaxNamespaceNameLength || !NamespaceNameRegex().IsMatch(namespaceName.Value))
            {
                throw new InvalidOperationException($"{settingName} contains an invalid namespace name: '{part}'.");
            }

            namespaces.Add(namespaceName);
        }

        return namespaces.Count == 0 ? null : namespaces;
    }

    // DNS-1123 label
    [GeneratedRegex("^[a-z0-9]([-a-z0-9]*[a-z0-9])?$")]
    private static partial Regex NamespaceNameRegex();
}
