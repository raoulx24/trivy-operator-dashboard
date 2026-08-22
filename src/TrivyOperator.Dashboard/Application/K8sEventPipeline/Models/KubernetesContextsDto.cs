namespace TrivyOperator.Dashboard.Application.K8sEventPipeline.Models;

public class KubernetesContextsDto
{
    public string[] Contexts { get; init; } = [];
    public string Current { get; init; } = string.Empty;
}
