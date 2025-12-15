using k8s;
using k8s.Autorest;
using TrivyOperator.Dashboard.Domain.Services.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.CustomResources.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Abstractions;

namespace TrivyOperator.Dashboard.Domain.Services;

public class ClusterScopedTrivyReportDomainService<TKubernetesObject>(
    IKubernetesClientFactory kubernetesClientFactory,
    IKubernetesContextProvider kubernetesContextProvider,
    ICustomResourceDefinitionFactory customResourceDefinitionFactory)
    : ClusterScopedResourceDomainService<TKubernetesObject, CustomResourceList<TKubernetesObject>>(
        kubernetesClientFactory, kubernetesContextProvider)
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
        CancellationToken? cancellationToken = null) => kubernetesClient
        .ListClusterCustomObjectAsync<CustomResourceList<TKubernetesObject>>(
            TrivyReportCrd.Group,
            TrivyReportCrd.Version,
            TrivyReportCrd.PluralName,
            limit: pageLimit,
            continueParameter: continueToken,
            cancellationToken: cancellationToken ?? CancellationToken.None);

    public override Task<TKubernetesObject>
        GetResource(string resourceName, CancellationToken? cancellationToken = null) => kubernetesClient
        .CustomObjects.GetClusterCustomObjectAsync<TKubernetesObject>(
            TrivyReportCrd.Group,
            TrivyReportCrd.Version,
            TrivyReportCrd.PluralName,
            resourceName,
            cancellationToken ?? CancellationToken.None);

    public override Task<HttpOperationResponse<CustomResourceList<TKubernetesObject>>> GetResourceWatchList(
        string? lastResourceVersion = null,
        int? timeoutSeconds = null,
        CancellationToken? cancellationToken = null) => kubernetesClient
        .CustomObjects.ListClusterCustomObjectWithHttpMessagesAsync<CustomResourceList<TKubernetesObject>>(
            TrivyReportCrd.Group,
            TrivyReportCrd.Version,
            TrivyReportCrd.PluralName,
            watch: true,
            resourceVersion: lastResourceVersion,
            allowWatchBookmarks: true,
            timeoutSeconds: timeoutSeconds,
            cancellationToken: cancellationToken ?? CancellationToken.None);
}
