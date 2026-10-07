using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Application.Alerts.Queries.Models;

namespace TrivyOperator.Dashboard.Api.Alerts.Serializations;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UseStringEnumConverter = true,
    GenerationMode = JsonSourceGenerationMode.Metadata
)]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(ValidationProblemDetails))]

[JsonSerializable(typeof(IEnumerable<AlertDto>))]

public partial class AlertsApiJsonContext : JsonSerializerContext;
