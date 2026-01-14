using k8s;
using k8s.Autorest;
using k8s.Models;
using TrivyOperator.Dashboard.Domain.K8s.Abstractions;
using TrivyOperator.Dashboard.Domain.K8s.UpstreamAbstractions;
using TrivyOperator.Dashboard.Domain.Trivy.CustomResources.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Services.K8sApi.Abstractions;

namespace TrivyOperator.Dashboard.Domain.Trivy.Services.K8sApi;

public class NamespacedTrivyReportDomainService<TKubernetesObject>(
    IKubernetesClientFactory kubernetesClientFactory,
    IServiceScopeFactory scopeFactory,
    ICustomResourceDefinitionFactory customResourceDefinitionFactory,
    IClusterScopedResourceQueryDomainService<V1Namespace, V1NamespaceList> namespaceDomainService
) : NamespacedResourceDomainService<TKubernetesObject, CustomResourceList<TKubernetesObject>>(
    kubernetesClientFactory,
    scopeFactory,
    namespaceDomainService
)
    where TKubernetesObject : CustomResource
{
    private CustomResourceDefinition? trivyReportCrd;

    protected CustomResourceDefinition TrivyReportCrd
    {
        get
        {
            trivyReportCrd ??= customResourceDefinitionFactory.Get<TKubernetesObject>();

            return trivyReportCrd;
        }
    }

    public override Task<CustomResourceList<TKubernetesObject>> GetResourceList(
        string namespaceName,
        int? pageLimit = null,
        string? continueToken = null,
        CancellationToken? cancellationToken = null
    ) => GetKubernetesClient()
        .ListNamespacedCustomObjectAsync<CustomResourceList<TKubernetesObject>>(
            TrivyReportCrd.Group,
            TrivyReportCrd.Version,
            namespaceName,
            TrivyReportCrd.PluralName,
            limit: pageLimit,
            continueParameter: continueToken,
            cancellationToken: cancellationToken ?? CancellationToken.None
        );

    public override Task<TKubernetesObject> GetResource(
        string resourceName,
        string namespaceName,
        CancellationToken? cancellationToken = null
    ) => GetKubernetesClient()
        .CustomObjects.GetNamespacedCustomObjectAsync<TKubernetesObject>(
            TrivyReportCrd.Group,
            TrivyReportCrd.Version,
            namespaceName,
            TrivyReportCrd.PluralName,
            resourceName,
            cancellationToken ?? CancellationToken.None
        );

    public override Task<HttpOperationResponse<CustomResourceList<TKubernetesObject>>> GetResourceWatchList(
        string namespaceName,
        string? lastResourceVersion = null,
        int? timeoutSeconds = null,
        CancellationToken? cancellationToken = null
    ) => GetKubernetesClient()
        .CustomObjects.ListNamespacedCustomObjectWithHttpMessagesAsync<CustomResourceList<TKubernetesObject>>(
            TrivyReportCrd.Group,
            TrivyReportCrd.Version,
            namespaceName,
            TrivyReportCrd.PluralName,
            watch: true,
            resourceVersion: lastResourceVersion,
            allowWatchBookmarks: true,
            timeoutSeconds: timeoutSeconds,
            cancellationToken: cancellationToken ?? CancellationToken.None
        );
}
