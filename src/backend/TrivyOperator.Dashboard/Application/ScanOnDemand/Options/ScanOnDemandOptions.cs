namespace TrivyOperator.Dashboard.Application.ScanOnDemand.Options;

// section "ScanOnDemand"; the lists are comma delimited, like Kubernetes:NamespaceList
public record ScanOnDemandOptions
{
    public bool Enabled { get; init; } = false;

    // created by the admin, not by the app; reserved, so it never reaches the main pipeline
    public string NamespaceName { get; init; } = "trivy-scans";

    // empty: the namespace's default service account
    public string ServiceAccountName { get; init; } = string.Empty;

    // optional; must exist in NamespaceName. The app references it by name and never reads it
    public string ImagePullSecretName { get; init; } = string.Empty;

    // the init container that blocks forever, so the scanned image never runs; owned by the settings, never by the
    // pod template: an init container that ends would let the scanned image run
    public string InitContainerImage { get; init; } = "registry.k8s.io/pause:3.10";

    // empty for pause (its entrypoint blocks); e.g. ["tail", "-f", "/dev/null"] for an image like alpine
    public string[] InitContainerCommand { get; init; } = [];

    // optional path to a bare Pod YAML (e.g. mounted from a ConfigMap); read once at startup; empty: the built-in spec
    // the app sets name, namespace, its labels, the containers' names, images and commands, restartPolicy,
    // automountServiceAccountToken and activeDeadlineSeconds; the rest is kept
    public string PodTemplateFile { get; init; } = string.Empty;

    // the ceiling: the user picks among these, never outside them (ScanReportType names)
    public string AllowedReportTypes { get; init; } = "VulnerabilityReport,ExposedSecretReport,SbomReport";

    // registry hosts, exact (myregistry.io) or with a leading wildcard (*.azurecr.io)
    // empty allow list: every registry is allowed; the deny list wins over the allow list
    public string RegistryAllowList { get; init; } = string.Empty;
    public string RegistryDenyList { get; init; } = string.Empty;

    // no selected report by then: the scan times out
    public int ScanTimeoutSeconds { get; init; } = 900;

    // after the first selected report, how long to wait for the other selected ones
    public int ReportsGracePeriodSeconds { get; init; } = 30;

    public int ResultTtlHours { get; init; } = 168;

    public int MaxConcurrentScans { get; init; } = 3;

    // the scans not finished yet (pending and running); a submit beyond it is refused
    public int MaxQueuedScans { get; init; } = 20;

    // how often the runner checks the scans; a submit wakes it up earlier
    public int PollIntervalSeconds { get; init; } = 5;
}
