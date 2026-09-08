using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;

namespace TrivyOperator.Dashboard.Api.Trivy.Serializations;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UseStringEnumConverter = true,
    GenerationMode = JsonSourceGenerationMode.Metadata
)]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(ValidationProblemDetails))]

[JsonSerializable(typeof(IEnumerable<ExposedSecretReportDto>))]
[JsonSerializable(typeof(ExposedSecretReportDto))]
[JsonSerializable(typeof(IEnumerable<ExposedSecretReportDenormalizedDto>))]
[JsonSerializable(typeof(IEnumerable<ExposedSecretReportImageDto>))]

public partial class ExposedSecretReportsApiJsonContext : JsonSerializerContext;
