using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Application.Nvd.Queries.Models;

namespace TrivyOperator.Dashboard.Api.Nvd.Serializations;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UseStringEnumConverter = true,
    GenerationMode = JsonSourceGenerationMode.Metadata
)]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(ValidationProblemDetails))]

[JsonSerializable(typeof(NvdCveDto))]
[JsonSerializable(typeof(NvdCvssDto))]
[JsonSerializable(typeof(NvdReferenceDto))]

public partial class NvdApiJsonContext : JsonSerializerContext;
