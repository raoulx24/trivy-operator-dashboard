using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Api.Serialization;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UseStringEnumConverter = true,
    GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(ValidationProblemDetails))]

public partial class ControllersApiJsonContext : JsonSerializerContext;