using k8s;
using k8s.Autorest;
using k8s.Models;
using Microsoft.Extensions.Options;
using System.Net;
using TrivyOperator.Dashboard.Application.Kubernetes.Contexts.Abstractions;
using TrivyOperator.Dashboard.Application.ScanOnDemand.Abstractions;
using TrivyOperator.Dashboard.Application.ScanOnDemand.Models;
using TrivyOperator.Dashboard.Application.ScanOnDemand.Options;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.ScanOnDemand.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.ClientFactory.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.ScanOnDemand.Kubernetes;

// the scan pod: an init container that never ends, so the scanned image is never pulled by the kubelet and never runs;
// trivy-operator sees the pod and scans the image itself
// always in the default context
public sealed class ScanPodClient(
    IKubernetesClientFactory kubernetesClientFactory,
    IContextProvider contextProvider,
    ScanPodTemplateProvider templateProvider,
    IOptions<ScanOnDemandOptions> options,
    ILogger<ScanPodClient> logger
) : IScanPodClient
{
    // init container waiting reasons the pod does not recover from
    private static readonly HashSet<string> FailedWaitingReasons = new(StringComparer.Ordinal)
    {
        "ErrImagePull",
        "ImagePullBackOff",
        "InvalidImageName",
        "CreateContainerConfigError",
        "CreateContainerError",
    };

    private ScanOnDemandOptions Options => options.Value;

    public async Task<ResourceName> CreatePodAsync(ScanId id, ImageReference image, CancellationToken ctx = default)
    {
        V1Pod? template;

        try
        {
            template = templateProvider.CreateBase();
        }
        catch (InvalidOperationException ex)
        {
            // a configuration problem: retrying does not help
            throw new ScanPodRejectedException(ex.Message, ex);
        }

        V1Pod pod = ScanPodSpecBuilder.Build(template, id, image, Options);

        try
        {
            await GetClient().CoreV1.CreateNamespacedPodAsync(pod, Options.NamespaceName, cancellationToken: ctx);
        }
        catch (HttpOperationException ex) when (ex.Response.StatusCode == HttpStatusCode.Conflict)
        {
            // created before a restart; the same scan id means the same pod
            logger.LogInformation("Scan pod {podName} already exists", pod.Metadata.Name);
        }
        // 4xx: the request itself is refused (service account, quota, admission, ...), retrying does not help
        // anything else (5xx, network) is thrown as is, so the caller can retry
        catch (HttpOperationException ex) when ((int)ex.Response.StatusCode is >= 400 and < 500)
        {
            throw new ScanPodRejectedException(GetStatusMessage(ex), ex);
        }

        return new ResourceName(pod.Metadata.Name);
    }

    public async Task<ScanPodStatus?> GetPodStatusAsync(ResourceName podName, CancellationToken ctx = default)
    {
        V1Pod pod;

        try
        {
            pod = await GetClient().CoreV1.ReadNamespacedPodAsync(podName.Value, Options.NamespaceName, cancellationToken: ctx);
        }
        catch (HttpOperationException ex) when (ex.Response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        return ToStatus(pod);
    }

    public async Task DeletePodAsync(ResourceName podName, CancellationToken ctx = default)
    {
        try
        {
            await GetClient().CoreV1.DeleteNamespacedPodAsync(
                podName.Value,
                Options.NamespaceName,
                gracePeriodSeconds: 0,
                cancellationToken: ctx);
        }
        catch (HttpOperationException ex) when (ex.Response.StatusCode == HttpStatusCode.NotFound)
        {
            // already gone
        }
    }

    public async Task<IReadOnlyList<ResourceName>> ListScanPodsAsync(CancellationToken ctx = default)
    {
        V1PodList list = await GetClient().CoreV1.ListNamespacedPodAsync(
            Options.NamespaceName,
            labelSelector: ScanPodLabels.Selector(),
            cancellationToken: ctx);

        return [.. (list.Items ?? []).Select(static x => new ResourceName(x.Metadata?.Name)).Where(static x => x.IsValid),];
    }

    private static ScanPodStatus ToStatus(V1Pod pod)
    {
        string phase = pod.Status?.Phase ?? "Unknown";

        if (pod.Metadata?.DeletionTimestamp is not null)
        {
            return new ScanPodStatus(phase, "Deleting", "The scan pod is being deleted.", true);
        }

        if (phase == "Failed")
        {
            return new ScanPodStatus(phase, pod.Status?.Reason, pod.Status?.Message, true);
        }

        V1ContainerState? initState = pod.Status?.InitContainerStatuses?
            .FirstOrDefault(static x => x.Name == ScanPodLabels.InitContainerName)?.State;

        if (initState?.Waiting is { } waiting)
        {
            return new ScanPodStatus(
                phase,
                waiting.Reason,
                waiting.Message,
                waiting.Reason is not null && FailedWaitingReasons.Contains(waiting.Reason));
        }

        // the init container is not supposed to end
        if (initState?.Terminated is { } terminated)
        {
            return new ScanPodStatus(
                phase,
                terminated.Reason ?? "InitContainerTerminated",
                terminated.Message ?? $"The init container ended with exit code {terminated.ExitCode}.",
                true);
        }

        V1PodCondition? scheduled = pod.Status?.Conditions?.FirstOrDefault(static x => x.Type == "PodScheduled");

        if (scheduled is not null && scheduled.Status == "False")
        {
            // reported only; the orchestrator decides how long to tolerate it
            return new ScanPodStatus(phase, scheduled.Reason, scheduled.Message, false);
        }

        return new ScanPodStatus(phase, pod.Status?.Reason, pod.Status?.Message, false);
    }

    private static string GetStatusMessage(HttpOperationException ex)
    {
        string? content = ex.Response.Content;

        if (!string.IsNullOrWhiteSpace(content))
        {
            try
            {
                V1Status? status = KubernetesJson.Deserialize<V1Status>(content);

                if (!string.IsNullOrWhiteSpace(status?.Message))
                {
                    return status.Message;
                }
            }
            catch (Exception)
            {
                // not a Status object; fall back to the HTTP status
            }
        }

        return $"{(int)ex.Response.StatusCode} {ex.Response.ReasonPhrase}";
    }

    private k8s.Kubernetes GetClient() => kubernetesClientFactory.GetClient(contextProvider.GetDefaultContext());
}
