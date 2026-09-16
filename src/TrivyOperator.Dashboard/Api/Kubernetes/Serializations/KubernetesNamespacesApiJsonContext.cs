using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Api.Kubernetes.Serializations;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UseStringEnumConverter = true,
    GenerationMode = JsonSourceGenerationMode.Metadata
)]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(ValidationProblemDetails))]

[JsonSerializable(typeof(IEnumerable<string>))]

public partial class KubernetesNamespacesApiJsonContext : JsonSerializerContext;
