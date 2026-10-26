using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Application.CveEnrichment.Queries.Models;

namespace TrivyOperator.Dashboard.Api.CveEnrichment.Serializations;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UseStringEnumConverter = true,
    GenerationMode = JsonSourceGenerationMode.Metadata
)]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(ValidationProblemDetails))]

[JsonSerializable(typeof(IReadOnlyList<CveInfoDto>))]
[JsonSerializable(typeof(CveInfoDto))]
[JsonSerializable(typeof(KevResultDto))]
[JsonSerializable(typeof(EpssResultDto))]
[JsonSerializable(typeof(EuvdResultDto))]

public partial class CveEnrichmentApiJsonContext : JsonSerializerContext;
