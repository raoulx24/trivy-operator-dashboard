using k8s;
using k8s.Models;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using TrivyOperator.Dashboard.Application.K8s.Services.Options;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Utils;
using TrivyOperator.Dashboard.Infrastructure.K8s.Services.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.K8s.Services;

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
    
    public async IAsyncEnumerable<WatchEvent<V1Namespace>> GetResourceWatchList(
    string? lastResourceVersion = null,
    int? timeoutSeconds = null,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        DateTimeOffset? deadline = timeoutSeconds is null
            ? null
            : DateTimeOffset.UtcNow.AddSeconds(timeoutSeconds.Value);

        if (string.IsNullOrEmpty(lastResourceVersion))
        {
            foreach (V1Namespace ns in await GetResources(cancellationToken))
            {
                ns.Metadata ??= new V1ObjectMeta();
                ns.Metadata.ResourceVersion = ResourceVersion;

                yield return new WatchEvent<V1Namespace>
                {
                    Type = WatchEventType.Added,
                    Object = ns,
                };
            }

            yield return GetBookmarkEvent();
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            TimeSpan delay = BookmarkInterval;

            if (deadline is not null)
            {
                TimeSpan remaining = deadline.Value - DateTimeOffset.UtcNow;

                if (remaining <= TimeSpan.Zero)
                    yield break;

                if (remaining < delay)
                    delay = remaining;
            }

            try
            {
                await Task.Delay(delay, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                yield break;
            }

            yield return GetBookmarkEvent();
        }
    }

    private static V1Namespace CreateNamespace(string namespaceName) => new()
    {
        ApiVersion = "v1",
        Kind = "Namespace",
        Metadata = new V1ObjectMeta
        {
            Name = namespaceName,
            Uid = GuidUtils.GetDeterministicGuid(namespaceName).ToString(),
        },
    };
    
    private static WatchEvent<V1Namespace> GetBookmarkEvent() =>
        new()
        {
            Type = WatchEventType.Bookmark,
            Object = new V1Namespace
            {
                Metadata = new V1ObjectMeta
                {
                    ResourceVersion = ResourceVersion,
                },
            },
        };
    
    private static readonly string ResourceVersion = "1";
    private static readonly TimeSpan BookmarkInterval = TimeSpan.FromSeconds(10);
}
