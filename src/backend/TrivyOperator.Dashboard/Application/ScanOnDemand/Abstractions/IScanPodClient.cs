using TrivyOperator.Dashboard.Application.ScanOnDemand.Models;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.ScanOnDemand.ValueObjects;

namespace TrivyOperator.Dashboard.Application.ScanOnDemand.Abstractions;

// the scan pod side; everything happens in the configured scan namespace
public interface IScanPodClient
{
    // ScanPodRejectedException when the API server refuses the pod (missing service account, quota, admission, ...);
    // other errors (API server or network) are thrown as they are and are worth a retry
    Task<ResourceName> CreatePodAsync(ScanId id, ImageReference image, CancellationToken ctx = default);

    // null when the pod no longer exists
    Task<ScanPodStatus?> GetPodStatusAsync(ResourceName podName, CancellationToken ctx = default);

    // no error when the pod is already gone
    Task DeletePodAsync(ResourceName podName, CancellationToken ctx = default);

    // the pods labeled as scan pods, for the leftover-pod sweeper
    Task<IReadOnlyList<ResourceName>> ListScanPodsAsync(CancellationToken ctx = default);
}
