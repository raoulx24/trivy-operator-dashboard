using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Application.Queries.AppVersions.Models;

namespace TrivyOperator.Dashboard.Api.AppVersions.Serializations;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UseStringEnumConverter = true,
    GenerationMode = JsonSourceGenerationMode.Metadata
)]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(ValidationProblemDetails))]

[JsonSerializable(typeof(IEnumerable<ReleaseDto>))]
[JsonSerializable(typeof(ReleaseDto))]
[JsonSerializable(typeof(AppVersion))]

public partial class AppVersionsApiJsonContext : JsonSerializerContext;
