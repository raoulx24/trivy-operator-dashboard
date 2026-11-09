using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Application.Home.Queries.Models;

namespace TrivyOperator.Dashboard.Api.Home.Serializations;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UseStringEnumConverter = true,
    GenerationMode = JsonSourceGenerationMode.Metadata
)]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(ValidationProblemDetails))]

[JsonSerializable(typeof(IReadOnlyList<string>))]
[JsonSerializable(typeof(HomeTotalsDto))]
[JsonSerializable(typeof(HomeActiveVulnerabilityDto))]
[JsonSerializable(typeof(HomeNewVulnerabilitiesDto))]
[JsonSerializable(typeof(HomeNewFixesDto))]
[JsonSerializable(typeof(HomeFixWaitingDto))]
[JsonSerializable(typeof(HomeKnownExploitedItemDto))]
[JsonSerializable(typeof(HomeKnownExploitedDto))]
[JsonSerializable(typeof(HomeActivityDayDto))]
[JsonSerializable(typeof(HomeActivityDto))]
[JsonSerializable(typeof(HomeBaseOsItemDto))]
[JsonSerializable(typeof(HomeBaseOsDto))]
[JsonSerializable(typeof(HomeFailingCheckDto))]
[JsonSerializable(typeof(HomeFailingChecksDto))]
[JsonSerializable(typeof(HomeOldestImageDto))]
[JsonSerializable(typeof(HomeOldestImagesDto))]
[JsonSerializable(typeof(HomePackageHotspotDto))]
[JsonSerializable(typeof(HomePackageHotspotsDto))]
[JsonSerializable(typeof(HomeReachEpssItemDto))]
[JsonSerializable(typeof(HomeReachEpssDto))]
[JsonSerializable(typeof(HomeWatcherIssueDto))]
[JsonSerializable(typeof(HomeReportFreshnessDto))]
[JsonSerializable(typeof(HomeSourceStatusDto))]
[JsonSerializable(typeof(HomeDataHealthDto))]

public partial class HomeApiJsonContext : JsonSerializerContext;
