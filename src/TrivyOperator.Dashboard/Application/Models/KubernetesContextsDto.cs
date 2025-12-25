namespace TrivyOperator.Dashboard.Application.Models;

public class KubernetesContextsDto
{
    public string[] Contexts { get; init; } = [];
    public string Current {  get; init; } = string.Empty;
}
