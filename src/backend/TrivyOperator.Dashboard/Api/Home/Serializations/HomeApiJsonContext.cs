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
[JsonSerializable(typeof(HomeSeverityCountsDto))]
[JsonSerializable(typeof(HomeHistoryDriftDayDto))]
[JsonSerializable(typeof(HomeHistoryDriftDto))]
[JsonSerializable(typeof(HomeHistoryDriftDenormalizedItemDto))]
[JsonSerializable(typeof(HomeHistoryDriftGroupDto))]
[JsonSerializable(typeof(HomeHistoryDriftDenormalizedDto))]
[JsonSerializable(typeof(HomeNewImagesDayDto))]
[JsonSerializable(typeof(HomeNewImagesDto))]
[JsonSerializable(typeof(HomeNewImagesDenormalizedItemDto))]
[JsonSerializable(typeof(HomeNewImagesGroupDto))]
[JsonSerializable(typeof(HomeNewImagesDenormalizedDto))]
[JsonSerializable(typeof(HomeBaseOsImageDto))]
[JsonSerializable(typeof(HomeBaseOsItemDto))]
[JsonSerializable(typeof(HomeBaseOsDto))]
[JsonSerializable(typeof(HomeBaseOsImageRowDto))]
[JsonSerializable(typeof(HomeBaseOsImagesDto))]
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

[JsonSerializable(typeof(HomeWhereIsVulnerabilityPackageDto))]
[JsonSerializable(typeof(HomeWhereIsVulnerabilitySeverityDto))]
[JsonSerializable(typeof(HomeWhereIsVulnerabilityItemDto))]
[JsonSerializable(typeof(HomeWhereIsVulnerabilityDetailItemDto))]
[JsonSerializable(typeof(HomeWhereIsVulnerabilityDetailDto))]
[JsonSerializable(typeof(HomeWhereIsVulnerabilityDto))]
[JsonSerializable(typeof(HomeWhereIsPackageItemDto))]
[JsonSerializable(typeof(HomeWhereIsPackageDto))]
[JsonSerializable(typeof(HomeWhereIsPackageVulnerabilityDto))]
[JsonSerializable(typeof(HomeWhereIsPackageDetailItemDto))]
[JsonSerializable(typeof(HomeWhereIsPackageDetailDto))]
[JsonSerializable(typeof(HomeWhereIsLicensePackageDto))]
[JsonSerializable(typeof(HomeWhereIsLicenseItemDto))]
[JsonSerializable(typeof(HomeWhereIsLicenseDto))]
[JsonSerializable(typeof(HomeWhereIsCheckResourceDto))]
[JsonSerializable(typeof(HomeWhereIsCheckItemDto))]
[JsonSerializable(typeof(HomeWhereIsCheckDto))]

public partial class HomeApiJsonContext : JsonSerializerContext;
