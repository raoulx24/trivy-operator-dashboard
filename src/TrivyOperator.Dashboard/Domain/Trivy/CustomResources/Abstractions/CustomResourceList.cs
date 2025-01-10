﻿using k8s;
using k8s.Models;
using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Domain.Trivy.CustomResources.Abstractions;

public class CustomResourceList<T> : IKubernetesObject<V1ListMeta>, IItems<T>
    where T : CustomResource
{
    [JsonPropertyName("metadata")]
    public V1ListMeta Metadata { get; set; } = new();
    [JsonPropertyName("items")]
    public IList<T> Items { get; set; } = [];
    public string ApiVersion { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
}
