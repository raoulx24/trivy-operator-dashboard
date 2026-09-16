namespace TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.Options;

public record KubernetesOptions
{
    public string KubeConfigFileName { get; init; } = string.Empty;
    public bool UseDefaultContext { get; set; } = true;
    public string NamespaceList { get; init; } = string.Empty;
}
