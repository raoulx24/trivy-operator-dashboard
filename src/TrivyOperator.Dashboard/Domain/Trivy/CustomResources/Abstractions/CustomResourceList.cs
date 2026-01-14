using k8s;
using k8s.Models;
using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Domain.Trivy.CustomResources.Abstractions;

public class CustomResourceList<T> : IKubernetesObject<V1ListMeta>, IItems<T>
    where T : CustomResource
{
    [JsonPropertyName("items")]
    public IList<T> Items { get; set; } = [];

    [JsonPropertyName("metadata")]
    public V1ListMeta Metadata { get; set; } = new();

    public string ApiVersion { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
}
