using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Application.CveDetails.Queries.Models;

namespace TrivyOperator.Dashboard.Api.CveDetails.Serializations;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UseStringEnumConverter = true,
    GenerationMode = JsonSourceGenerationMode.Metadata
)]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(ValidationProblemDetails))]

[JsonSerializable(typeof(CveDetailsDto))]
[JsonSerializable(typeof(CveDetailsCvssDto))]
[JsonSerializable(typeof(CveDetailsReferenceDto))]

public partial class CveDetailsApiJsonContext : JsonSerializerContext;
