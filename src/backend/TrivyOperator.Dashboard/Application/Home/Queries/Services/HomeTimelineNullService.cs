using TrivyOperator.Dashboard.Application.Home.Queries.Models;
using TrivyOperator.Dashboard.Application.Home.Queries.Services.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.Queries.Services;

namespace TrivyOperator.Dashboard.Application.Home.Queries.Services;

// multi-context and file repository: they get a different home page, these timelines have nothing to return
public class HomeTimelineNullService : QueryNullService, IHomeTimelineService
{
    private const string Reason = "The home page data is available only for the default context";

    public Task<HomeHistoryDriftDto> GetHistoryDrift(int? days, HomeFilter filter, CancellationToken ctx = default) =>
        Task.FromResult(new HomeHistoryDriftDto(false, Reason, DateOnly.FromDateTime(DateTime.UtcNow), []));

    public Task<HomeHistoryDriftDenormalizedDto> GetHistoryDriftDenormalized(
        int? days,
        HomeFilter filter,
        CancellationToken ctx = default) =>
        Task.FromResult(
            new HomeHistoryDriftDenormalizedDto(false, Reason, DateOnly.FromDateTime(DateTime.UtcNow), [], []));

    public Task<HomeNewImagesDto> GetNewImages(int? days, HomeFilter filter, CancellationToken ctx = default) =>
        Task.FromResult(new HomeNewImagesDto(false, Reason, DateOnly.FromDateTime(DateTime.UtcNow), null, []));

    public Task<HomeNewVulnerabilitiesSummaryDto> GetNewVulnerabilitiesSummary(
        int? days,
        HomeFilter filter,
        CancellationToken ctx = default) =>
        Task.FromResult(new HomeNewVulnerabilitiesSummaryDto(false, Reason, DateOnly.FromDateTime(DateTime.UtcNow), []));

    public Task<HomeNewFixesSummaryDto> GetNewFixesSummary(int? days, HomeFilter filter, CancellationToken ctx = default) =>
        Task.FromResult(new HomeNewFixesSummaryDto(false, Reason, DateOnly.FromDateTime(DateTime.UtcNow), []));

    public Task<HomeKnownExploitedSummaryDto> GetKnownExploitedSummary(
        int? days,
        HomeFilter filter,
        CancellationToken ctx = default) =>
        Task.FromResult(new HomeKnownExploitedSummaryDto(false, Reason, DateOnly.FromDateTime(DateTime.UtcNow), []));

    public Task<HomeNewImagesDenormalizedDto> GetNewImagesDenormalized(
        int? days,
        HomeFilter filter,
        CancellationToken ctx = default) =>
        Task.FromResult(
            new HomeNewImagesDenormalizedDto(false, Reason, DateOnly.FromDateTime(DateTime.UtcNow), [], []));
}
