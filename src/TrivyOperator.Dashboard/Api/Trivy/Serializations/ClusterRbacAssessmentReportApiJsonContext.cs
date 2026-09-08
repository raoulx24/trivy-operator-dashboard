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

[JsonSerializable(typeof(IEnumerable<ClusterRbacAssessmentReportDto>))]
[JsonSerializable(typeof(ClusterRbacAssessmentReportDto))]
[JsonSerializable(typeof(IEnumerable<ClusterRbacAssessmentReportDenormalizedDto>))]

public partial class ClusterRbacAssessmentReportApiJsonContext : JsonSerializerContext;
