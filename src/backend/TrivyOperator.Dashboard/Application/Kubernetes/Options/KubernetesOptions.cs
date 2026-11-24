namespace TrivyOperator.Dashboard.Application.Kubernetes.Options;

public record KubernetesOptions
{
    public string KubeConfigFileName { get; init; } = string.Empty;
    public bool UseDefaultContext { get; set; } = true;
    public string NamespaceList { get; init; } = string.Empty;
    // comma delimited; events from these namespaces never reach the main pipeline
    public string ExcludedNamespaceList { get; init; } = string.Empty;
}
