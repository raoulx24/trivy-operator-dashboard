using TrivyOperator.Dashboard.Application.Home.Queries.Models;
using TrivyOperator.Dashboard.Application.Home.Queries.Services.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.Queries.Services;

namespace TrivyOperator.Dashboard.Application.Home.Queries.Services;

// multi-context and file repository: they get a different home page, these timelines have nothing to return
public class HomeTimelineNullService : QueryNullService, IHomeTimelineService
{
    private const string Reason = "The home page data is available only for the default context";

    public Task<HomeHistoryDriftDto> GetHistoryDrift(int? days, HomeFilter filter, CancellationToken ctx = default) =>
        Task.FromResult(new HomeHistoryDriftDto(false, Reason, DateOnly.FromDateTime(DateTime.UtcNow), [], []));

    public Task<HomeNewImagesDto> GetNewImages(int? days, HomeFilter filter, CancellationToken ctx = default) =>
        Task.FromResult(new HomeNewImagesDto(false, Reason, DateOnly.FromDateTime(DateTime.UtcNow), null, [], []));
}
