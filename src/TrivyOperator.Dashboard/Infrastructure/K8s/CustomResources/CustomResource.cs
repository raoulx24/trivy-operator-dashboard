using k8s;
using k8s.Models;
using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Infrastructure.K8s.CustomResources;

public abstract class CustomResource : IKubernetesObject<V1ObjectMeta>
{
    [JsonPropertyName("metadata")]
    public V1ObjectMeta Metadata { get; set; } = new();

    public string ApiVersion { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
}
