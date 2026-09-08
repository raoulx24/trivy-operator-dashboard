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

[JsonSerializable(typeof(IEnumerable<ClusterComplianceReportDto>))]
[JsonSerializable(typeof(ClusterComplianceReportDto))]
[JsonSerializable(typeof(IEnumerable<ClusterComplianceReportDenormalizedDto>))]

public partial class ClusterComplianceReportsApiJsonContext : JsonSerializerContext;
