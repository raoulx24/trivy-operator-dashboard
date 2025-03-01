﻿using k8s.Autorest;
using System.Net;
using TrivyOperator.Dashboard.Application.Services.WatcherStates;

namespace TrivyOperator.Dashboard.Application.Models;

public class WatcherStateInfoDto
{
    public string KubernetesObjectType { get; init; } = string.Empty;
    public string? NamespaceName { get; init; }
    public string Status { get; init; } = string.Empty;
    public string MitigationMessage { get; init; } = string.Empty;
    public string? LastException { get; init; }
    public DateTime? LastEventMoment { get; init; }
}

public static class WatcherStateInfoExtensions
{
    public static WatcherStateInfoDto ToWatcherStateInfoDto(this WatcherStateInfo? watcherStateInfo) =>
        watcherStateInfo == null
            ? new WatcherStateInfoDto()
            : new WatcherStateInfoDto
            {
                KubernetesObjectType = watcherStateInfo.WatchedKubernetesObjectType.Name,
                NamespaceName = watcherStateInfo.WatcherKey,
                Status = watcherStateInfo.Status.ToString(),
                MitigationMessage = GetMitigationMessage(watcherStateInfo),
                LastException = watcherStateInfo.LastException?.Message ?? string.Empty,
                LastEventMoment = watcherStateInfo.LastEventMoment,
            };

    private static string GetMitigationMessage(WatcherStateInfo watcherStateInfo)
    {
        if (watcherStateInfo.LastException == null)
            return "All ok";
        if (((HttpOperationException)watcherStateInfo.LastException).Response.StatusCode == HttpStatusCode.Unauthorized)
            return "Unauthorized: The kube config file does not provide a porper token";
        if (((HttpOperationException)watcherStateInfo.LastException).Response.StatusCode == HttpStatusCode.Forbidden)
            return "Forbidden: The k8s user is not allowed to perform the watch operation";
        if (((HttpOperationException)watcherStateInfo.LastException).Response.StatusCode == HttpStatusCode.NotFound)
            return "Not Found: The specified resource type does not exist in cluster (it might be that Trivy is not installed)";

        return "Unknown mitigation";
    }
}
