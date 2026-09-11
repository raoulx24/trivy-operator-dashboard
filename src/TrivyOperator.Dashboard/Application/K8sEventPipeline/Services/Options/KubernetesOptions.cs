namespace TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.Options;

public record KubernetesOptions
{
    public string KubeConfigFileName { get; init; } = string.Empty;
    public bool UseDefaultContext { get; set; } = true;
    public string NamespaceList { get; init; } = string.Empty;
}
