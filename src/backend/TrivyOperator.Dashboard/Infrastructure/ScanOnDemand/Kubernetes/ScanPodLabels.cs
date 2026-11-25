using TrivyOperator.Dashboard.Domain.ScanOnDemand.ValueObjects;

namespace TrivyOperator.Dashboard.Infrastructure.ScanOnDemand.Kubernetes;

internal static class ScanPodLabels
{
    public const string ManagedBy = "app.kubernetes.io/managed-by";
    public const string ManagedByValue = "trivy-operator-dashboard";
    public const string ScanId = "trivy-operator-dashboard/scan-id";

    // the container that references the scanned image; the reports of the init container are ignored
    public const string TargetContainerName = "target";
    public const string InitContainerName = "wait-forever";

    private const string PodNamePrefix = "sod-";

    // every scan pod, for the leftover-pod sweeper
    public static string Selector() => $"{ManagedBy}={ManagedByValue}";

    // 4 + 32 characters, a valid DNS-1123 label
    public static string PodName(ScanId id) => $"{PodNamePrefix}{id.Value}";
}
