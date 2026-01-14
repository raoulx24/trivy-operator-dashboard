using k8s;
using k8s.Autorest;
using TrivyOperator.Dashboard.Domain.K8s.Abstractions;
using TrivyOperator.Dashboard.Domain.K8s.UpstreamAbstractions;
using TrivyOperator.Dashboard.Domain.Trivy.CustomResources.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Services.K8sApi.Abstractions;

namespace TrivyOperator.Dashboard.Domain.Trivy.Services.K8sApi;

public class ClusterScopedTrivyReportDomainService<TKubernetesObject>(
    IKubernetesClientFactory kubernetesClientFactory,
    IServiceScopeFactory scopeFactory,
    ICustomResourceDefinitionFactory customResourceDefinitionFactory
) : ClusterScopedResourceDomainService<TKubernetesObject, CustomResourceList<TKubernetesObject>>(
    kubernetesClientFactory,
    scopeFactory
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
        int? pageLimit = null,
        string? continueToken = null,
        CancellationToken? cancellationToken = null
    ) => GetKubernetesClient()
        .ListClusterCustomObjectAsync<CustomResourceList<TKubernetesObject>>(
            TrivyReportCrd.Group,
            TrivyReportCrd.Version,
            TrivyReportCrd.PluralName,
            limit: pageLimit,
            continueParameter: continueToken,
            cancellationToken: cancellationToken ?? CancellationToken.None
        );

    public override Task<TKubernetesObject>
        GetResource(string resourceName, CancellationToken? cancellationToken = null) => GetKubernetesClient()
        .CustomObjects.GetClusterCustomObjectAsync<TKubernetesObject>(
            TrivyReportCrd.Group,
            TrivyReportCrd.Version,
            TrivyReportCrd.PluralName,
            resourceName,
            cancellationToken ?? CancellationToken.None
        );

    public override Task<HttpOperationResponse<CustomResourceList<TKubernetesObject>>> GetResourceWatchList(
        string? lastResourceVersion = null,
        int? timeoutSeconds = null,
        CancellationToken? cancellationToken = null
    ) => GetKubernetesClient()
        .CustomObjects.ListClusterCustomObjectWithHttpMessagesAsync<CustomResourceList<TKubernetesObject>>(
            TrivyReportCrd.Group,
            TrivyReportCrd.Version,
            TrivyReportCrd.PluralName,
            watch: true,
            resourceVersion: lastResourceVersion,
            allowWatchBookmarks: true,
            timeoutSeconds: timeoutSeconds,
            cancellationToken: cancellationToken ?? CancellationToken.None
        );
}
