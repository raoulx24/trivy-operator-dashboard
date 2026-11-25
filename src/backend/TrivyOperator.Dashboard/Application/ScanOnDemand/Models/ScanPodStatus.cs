namespace TrivyOperator.Dashboard.Application.ScanOnDemand.Models;

// the scan pod's phase plus the reason/message of its init container or of the pod itself
// IsFailed: the pod cannot recover (init image cannot be pulled, unschedulable for too long, deleted, ...)
// note: the scanned image is never pulled by the kubelet (the init container never ends), so its pull
// problems never show here; they happen in trivy's scan job
public sealed record ScanPodStatus(string Phase, string? Reason, string? Message, bool IsFailed);
