using k8s;
using k8s.Models;
using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.Kubernetes.Options;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Services.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.StaticResources.Services;

public class StaticNamespaceService(
    IOptions<KubernetesOptions> kubernetesOptions,
    ILogger<StaticNamespaceService> logger
) : IClusterScopedResourceService<V1Namespace, V1NamespaceList>
{
    public Task<IList<V1Namespace>> GetResources(CancellationToken cancellationToken = default)
    {
        string configKubernetesNamespaces = kubernetesOptions.Value.NamespaceList;

        if (string.IsNullOrWhiteSpace(configKubernetesNamespaces))
        {
            throw new ArgumentNullException(
                "Provided parameter Kubernetes.NamespaceList is null or whitespace.",
                (Exception?)null
            );
        }

        List<V1Namespace> kubernetesNamespaces =
            [.. configKubernetesNamespaces.Split(',').Select(namespaceName => CreateNamespace(namespaceName.Trim())),];
        logger.LogDebug("Found {listCount} kubernetes namespace names.", kubernetesNamespaces.Count);

        return Task.FromResult<IList<V1Namespace>>(kubernetesNamespaces);
    }

    public ContextName GetCurrentContext() => new();

    public Task<V1Namespace> GetResource(string resourceName, CancellationToken cancellationToken = default) =>
        Task.FromResult(CreateNamespace(resourceName));

    public async Task<V1NamespaceList> GetResourceList(
        int? pageLimit = null,
        string? continueToken = null,
        CancellationToken cancellationToken = default
    ) => new()
    {
        ApiVersion = "v1",
        Kind = "NamespaceList",
        Metadata = new V1ListMeta
        {
            ResourceVersion = "1",
        },
        Items = await GetResources(cancellationToken),
    };
    
    // static namespaces are not watched: the report watchers are started per configured namespace
    public Task<ResourcePage<V1Namespace>> GetResourcePage(
        int? pageLimit = null,
        string? continueToken = null,
        CancellationToken cancellationToken = default
    ) => throw new NotSupportedException("Static namespaces are not watched.");

    public IAsyncEnumerable<WatchEvent<V1Namespace>> GetResourceWatchList(
        string? lastResourceVersion = null,
        int? timeoutSeconds = null,
        CancellationToken cancellationToken = default
    ) => throw new NotSupportedException("Static namespaces are not watched.");

    private static V1Namespace CreateNamespace(string namespaceName) => new()
    {
        ApiVersion = "v1",
        Kind = "Namespace",
        Metadata = new V1ObjectMeta
        {
            Name = namespaceName,
            Uid = Guid.NewGuid().ToString(),
        },
    };
}
