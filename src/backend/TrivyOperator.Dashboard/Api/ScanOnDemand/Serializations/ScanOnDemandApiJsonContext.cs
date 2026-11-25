using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Application.ScanOnDemand.Queries.Models;

namespace TrivyOperator.Dashboard.Api.ScanOnDemand.Serializations;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UseStringEnumConverter = true,
    GenerationMode = JsonSourceGenerationMode.Metadata
)]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(ValidationProblemDetails))]

[JsonSerializable(typeof(ScanSubmitRequestDto))]
[JsonSerializable(typeof(ScanSubmitResponseDto))]
[JsonSerializable(typeof(OnDemandScanDto))]
[JsonSerializable(typeof(IReadOnlyList<OnDemandScanDto>))]
[JsonSerializable(typeof(OnDemandScanResultDto))]

public partial class ScanOnDemandApiJsonContext : JsonSerializerContext;
