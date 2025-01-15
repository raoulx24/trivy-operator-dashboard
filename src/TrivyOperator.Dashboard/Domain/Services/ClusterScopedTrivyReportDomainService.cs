﻿using k8s;
using k8s.Autorest;
using TrivyOperator.Dashboard.Domain.Services.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.CustomResources.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Abstractions;

namespace TrivyOperator.Dashboard.Domain.Services;

public class ClusterScopedTrivyReportDomainService<TKubernetesObject>(IKubernetesClientFactory kubernetesClientFactory,
    ICustomResourceDefinitionFactory customResourceDefinitionFactory)
        : ClusterScopedResourceDomainService<TKubernetesObject, CustomResourceList<TKubernetesObject>>(kubernetesClientFactory)
        where TKubernetesObject : CustomResource
{   
    protected CustomResourceDefinition trivyReportCrd
    {
        get
        {
            if (_trivyReportCrd == null)
            {
                _trivyReportCrd = customResourceDefinitionFactory.Get<TKubernetesObject>();
            }
            return _trivyReportCrd;
        }
    }
    private CustomResourceDefinition? _trivyReportCrd;

    public override async Task<CustomResourceList<TKubernetesObject>> GetResourceList(int? pageLimit = null, string? continueToken = null, CancellationToken? cancellationToken = null)
    {
        return await kubernetesClientFactory.GetClient()
            .ListClusterCustomObjectAsync<CustomResourceList<TKubernetesObject>>(
                trivyReportCrd.Group,
                trivyReportCrd.Version,
                trivyReportCrd.PluralName,
                limit: pageLimit,
                continueParameter: continueToken,
                cancellationToken: cancellationToken ?? new());
    }

    public override async Task<TKubernetesObject> GetResource(string resourceName, CancellationToken? cancellationToken = null)
    {
        return await kubernetesClientFactory.GetClient()
            .CustomObjects.GetClusterCustomObjectAsync<TKubernetesObject>(
                trivyReportCrd.Group,
                trivyReportCrd.Version,
                trivyReportCrd.PluralName,
                resourceName,
                cancellationToken: cancellationToken ?? new());
    }

    public override async Task<HttpOperationResponse<CustomResourceList<TKubernetesObject>>> GetResourceWatchList(
        string? lastResourceVersion = null,
        int? timeoutSeconds = null,
        CancellationToken? cancellationToken = null)
    {
        return await kubernetesClientFactory.GetClient()
            .CustomObjects
            .ListClusterCustomObjectWithHttpMessagesAsync<CustomResourceList<TKubernetesObject>>(
                trivyReportCrd.Group,
                trivyReportCrd.Version,
                trivyReportCrd.PluralName,
                watch: true,
                resourceVersion: lastResourceVersion,
                allowWatchBookmarks: true,
                timeoutSeconds: timeoutSeconds,
                cancellationToken: cancellationToken ?? new());
    }
}
