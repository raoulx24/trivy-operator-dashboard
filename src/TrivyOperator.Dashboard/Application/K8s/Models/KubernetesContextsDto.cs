namespace TrivyOperator.Dashboard.Application.K8s.Models;

public class KubernetesContextsDto
{
    public string[] Contexts { get; init; } = [];
    public string Current { get; init; } = string.Empty;
}
