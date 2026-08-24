using k8s.Autorest;
using System.Net;
using TrivyOperator.Dashboard.Application.K8s.Services.WatcherStates;
using TrivyOperator.Dashboard.Application.WatcherStates.Models;

namespace TrivyOperator.Dashboard.Application.Queries.WatcherStates.Models;

public class WatcherStatusDto
{
    public string KubernetesObjectType { get; init; } = string.Empty;
    public string ContextName { get; init; } = string.Empty;
    public string? NamespaceName { get; init; }
    public string Status { get; init; } = string.Empty;
    public string MitigationMessage { get; init; } = string.Empty;
    public string? LastException { get; init; }
    public DateTime? LastEventMoment { get; init; }
    public long EventsGauge { get; init; }
}

public class RecreateWatcherRequest
{
    public string KubernetesObjectType { get; init; } = string.Empty;
    public string ContextName { get; init; } = string.Empty;
    public string NamespaceName { get; init; } = string.Empty;
}

public class RecreateWatcherResponse
{
    public string Message { get; set; } = string.Empty;
    public string? Error { get; set; }
    public string? KubernetesObjectType { get; set; }
    public string? NamespaceName { get; set; }
}

public static class WatcherStatusExtensions
{
    public static WatcherStatusDto ToWatcherStatusDto(this WatcherStateInfo? watcherStateInfo) =>
        watcherStateInfo == null ? new WatcherStatusDto() : new WatcherStatusDto
        {
            KubernetesObjectType = watcherStateInfo.WatchedKubernetesObjectType.Name,
            ContextName = watcherStateInfo.Key.ContextName.IsUnset
                ? string.Empty
                : watcherStateInfo.Key.ContextName.Value,
            NamespaceName = watcherStateInfo.Key.NamespaceName.IsClusterScoped
                ? string.Empty
                : watcherStateInfo.Key.NamespaceName.Value,
            Status = watcherStateInfo.Status.ToString(),
            MitigationMessage = GetMitigationMessage(watcherStateInfo),
            LastException = watcherStateInfo.LastException?.Message ?? string.Empty,
            LastEventMoment = watcherStateInfo.LastEventMoment,
            EventsGauge = watcherStateInfo.EventsGauge ?? -1,
        };

    private static string GetMitigationMessage(WatcherStateInfo watcherStateInfo)
    {
        if (watcherStateInfo.LastException == null)
        {
            return "All ok";
        }

        if (watcherStateInfo.LastException is HttpOperationException httpOpException)
        {
            if (httpOpException.Response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return "Unauthorized: The kube config file does not provide a proper token. Check file";
            }

            if (httpOpException.Response.StatusCode == HttpStatusCode.Forbidden)
            {
                return "Forbidden: The k8s user is not allowed to perform the watch operation. Check RBAC";
            }

            if (httpOpException.Response.StatusCode == HttpStatusCode.NotFound)
            {
                return "Not Found: The specified resource type does not exist in cluster (is Trivy installed?)";
            }
        }

        return "Unknown mitigation";
    }
}
