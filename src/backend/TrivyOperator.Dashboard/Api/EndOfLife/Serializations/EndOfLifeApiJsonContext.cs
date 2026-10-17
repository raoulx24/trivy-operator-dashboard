using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Application.EndOfLife.Queries.Models;

namespace TrivyOperator.Dashboard.Api.EndOfLife.Serializations;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UseStringEnumConverter = true,
    GenerationMode = JsonSourceGenerationMode.Metadata
)]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(ValidationProblemDetails))]

[JsonSerializable(typeof(IReadOnlyList<EolInfoDto>))]
[JsonSerializable(typeof(EolInfoDto))]
[JsonSerializable(typeof(EolReleaseDto))]

public partial class EndOfLifeApiJsonContext : JsonSerializerContext;
