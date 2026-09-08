using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Application.Queries.WatcherStates.Models;

namespace TrivyOperator.Dashboard.Api.K8s.Serializations;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UseStringEnumConverter = true,
    GenerationMode = JsonSourceGenerationMode.Metadata
)]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(ValidationProblemDetails))]

[JsonSerializable(typeof(IEnumerable<WatcherStatusDto>))]
[JsonSerializable(typeof(RecreateWatcherRequest))]
[JsonSerializable(typeof(RecreateWatcherResponse))]

public partial class WatcherStatusApiJsonContext : JsonSerializerContext;
