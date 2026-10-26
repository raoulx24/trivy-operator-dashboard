using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.CveEnrichment.Abstractions;
using TrivyOperator.Dashboard.Application.CveEnrichment.Models;
using TrivyOperator.Dashboard.Application.CveEnrichment.Options;
using TrivyOperator.Dashboard.Application.Shared.Cache.Abstractions;
using TrivyOperator.Dashboard.Domain.CveEnrichment.Entities;
using TrivyOperator.Dashboard.Domain.CveEnrichment.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

namespace TrivyOperator.Dashboard.Application.CveEnrichment.Services;

// EUVD has no validator for its search results, so every refresh downloads all the pages
public sealed class EuvdTimedHostedService(
    IEuvdProvider provider,
    IOptionsMonitor<CveEnrichmentOptions> options,
    ICveEnrichmentRefreshTrigger trigger,
    ICache<CveId, Timestamp> subjects,
    ICache<CveId, CveInfo> infos,
    ILogger<EuvdTimedHostedService> logger
) : CveCatalogTimedHostedService<EuvdEntry>(trigger, subjects, infos, logger)
{
    protected override CveSource Source => CveSources.Euvd;
    protected override TimeSpan ProcessingInterval => FromSeconds(options.CurrentValue.Euvd.ProcessingIntervalInSeconds);
    protected override TimeSpan RefreshInterval => FromMinutes(options.CurrentValue.Euvd.RefreshIntervalInMinutes);
    protected override TimeSpan ErrorTtl => FromMinutes(options.CurrentValue.Euvd.ErrorTtlInMinutes);

    protected override Task<CveProviderResult<EuvdEntry>> FetchCatalog(string? validator, CancellationToken ctx) =>
        provider.GetExploited(options.CurrentValue.Euvd.BaseUrl, options.CurrentValue.Euvd.PageSize, ctx);

    protected override CveId GetId(EuvdEntry entry) => entry.Id;

    protected override CveSourceResult<EuvdEntry>? GetPart(CveInfo info) => info.Euvd;

    protected override CveInfo WithPart(CveInfo info, CveSourceResult<EuvdEntry> part) => info with { Euvd = part, };
}
