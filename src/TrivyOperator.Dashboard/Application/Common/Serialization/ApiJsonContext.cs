using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Api.AppVersions.Models;
using TrivyOperator.Dashboard.Api.BackendSettings.Models;
using TrivyOperator.Dashboard.Application.K8s.Models;
using TrivyOperator.Dashboard.Application.Trivy.Models;

namespace TrivyOperator.Dashboard.Application.Common.Serialization;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UseStringEnumConverter = true,
    GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(ValidationProblemDetails))]

[JsonSerializable(typeof(VulnerabilityReportImageDto))]
[JsonSerializable(typeof(VulnerabilityReportImageDto[]))]
[JsonSerializable(typeof(IEnumerable<VulnerabilityReportImageDto>))]
[JsonSerializable(typeof(VulnerabilityReportDetailDto))]
[JsonSerializable(typeof(VulnerabilityReportDetailDto[]))]
[JsonSerializable(typeof(IEnumerable<VulnerabilityReportDetailDto>))]
[JsonSerializable(typeof(VulnerabilityReportDenormalizedDto))]


[JsonSerializable(typeof(BackendSettingsDto))]
[JsonSerializable(typeof(KubernetesContextsDto))]
[JsonSerializable(typeof(AppVersion))]

// ...
public partial class ApiJsonContext : JsonSerializerContext;