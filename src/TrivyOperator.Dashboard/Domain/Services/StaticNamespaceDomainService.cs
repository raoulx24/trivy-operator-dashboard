using k8s.Models;
using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.Services.Options;
using TrivyOperator.Dashboard.Domain.Services.Abstractions;
using TrivyOperator.Dashboard.Utils;

namespace TrivyOperator.Dashboard.Domain.Services;

public class StaticNamespaceDomainService(
    IOptions<KubernetesOptions> kubernetesOptions,
    ILogger<StaticNamespaceDomainService> logger)
    : IClusterScopedResourceQueryDomainService<V1Namespace, V1NamespaceList>
{
    public Task<IList<V1Namespace>> GetResources(CancellationToken? cancellationToken = null)
    {
        string configKubernetesNamespaces = kubernetesOptions.Value.NamespaceList;

        if (string.IsNullOrWhiteSpace(configKubernetesNamespaces))
        {
            throw new ArgumentNullException(
                "Provided parameter Kubernetes.NamespaceList is null or whitespace.",
                (Exception?)null);
        }

        List<V1Namespace> kubernetesNamespaces = [.. configKubernetesNamespaces
            .Split(',')
            .Select(namespaceName => CreateNamespace(namespaceName.Trim()))];
        logger.LogDebug("Found {listCount} kubernetes namespace names.", kubernetesNamespaces.Count);

        return Task.FromResult<IList<V1Namespace>>(kubernetesNamespaces);
    }

    public Task<V1Namespace> GetResource(string resourceName, CancellationToken? cancellationToken = null) =>
        Task.FromResult(CreateNamespace(resourceName));

    public Task<V1NamespaceList> GetResourceList(
        int? pageLimit = null,
        string? continueToken = null,
        CancellationToken? cancellationToken = null) => Task.FromResult(
        new V1NamespaceList
        {
            ApiVersion = "v1",
            Kind = "NamespaceList",
            Metadata = new V1ListMeta { ResourceVersion = "1", },
            Items = GetResources().Result,
        });
    
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
}
