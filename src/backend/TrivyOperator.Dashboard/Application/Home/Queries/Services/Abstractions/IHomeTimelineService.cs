using TrivyOperator.Dashboard.Application.Home.Queries.Models;
using TrivyOperator.Dashboard.Application.Shared.Queries.Services.Abstractions;

namespace TrivyOperator.Dashboard.Application.Home.Queries.Services.Abstractions;

// the per day timelines of the home page, default context only (see HomeTimelineDtos)
// days: the range, ending today (UTC); null = the default of the timeline
public interface IHomeTimelineService : IQueryService
{
    // default range: the history retention (KeepDays), beyond it the deltas are not complete
    Task<HomeHistoryDriftDto> GetHistoryDrift(int? days, HomeFilter filter, CancellationToken ctx = default);

    // default range: since the history start
    Task<HomeNewImagesDto> GetNewImages(int? days, HomeFilter filter, CancellationToken ctx = default);
}
