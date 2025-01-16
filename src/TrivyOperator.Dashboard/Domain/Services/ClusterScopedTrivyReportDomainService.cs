﻿using k8s;
using k8s.Autorest;
using TrivyOperator.Dashboard.Domain.Services.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.CustomResources.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Abstractions;

namespace TrivyOperator.Dashboard.Domain.Services;

public class ClusterScopedTrivyReportDomainService<TKubernetesObject>(
    IKubernetesClientFactory kubernetesClientFactory,
    ICustomResourceDefinitionFactory customResourceDefinitionFactory)
    : ClusterScopedResourceDomainService<TKubernetesObject, CustomResourceList<TKubernetesObject>>(
        kubernetesClientFactory) where TKubernetesObject : CustomResource
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

    public override async Task<CustomResourceList<TKubernetesObject>> GetResourceList(
        int? pageLimit = null,
        string? continueToken = null,
        CancellationToken? cancellationToken = null) => await kubernetesClientFactory.GetClient()
        .ListClusterCustomObjectAsync<CustomResourceList<TKubernetesObject>>(
            TrivyReportCrd.Group,
            TrivyReportCrd.Version,
            TrivyReportCrd.PluralName,
            limit: pageLimit,
            continueParameter: continueToken,
            cancellationToken: cancellationToken ?? new CancellationToken());

    public override async Task<TKubernetesObject>
        GetResource(string resourceName, CancellationToken? cancellationToken = null) => await kubernetesClientFactory
        .GetClient()
        .CustomObjects.GetClusterCustomObjectAsync<TKubernetesObject>(
            TrivyReportCrd.Group,
            TrivyReportCrd.Version,
            TrivyReportCrd.PluralName,
            resourceName,
            cancellationToken ?? new CancellationToken());

    public override async Task<HttpOperationResponse<CustomResourceList<TKubernetesObject>>> GetResourceWatchList(
        string? lastResourceVersion = null,
        int? timeoutSeconds = null,
        CancellationToken? cancellationToken = null) => await kubernetesClientFactory.GetClient()
        .CustomObjects.ListClusterCustomObjectWithHttpMessagesAsync<CustomResourceList<TKubernetesObject>>(
            TrivyReportCrd.Group,
            TrivyReportCrd.Version,
            TrivyReportCrd.PluralName,
            watch: true,
            resourceVersion: lastResourceVersion,
            allowWatchBookmarks: true,
            timeoutSeconds: timeoutSeconds,
            cancellationToken: cancellationToken ?? new CancellationToken());
}
