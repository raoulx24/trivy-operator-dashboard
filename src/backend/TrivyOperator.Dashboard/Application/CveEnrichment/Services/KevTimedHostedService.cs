using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.CveEnrichment.Abstractions;
using TrivyOperator.Dashboard.Application.CveEnrichment.Models;
using TrivyOperator.Dashboard.Application.CveEnrichment.Options;
using TrivyOperator.Dashboard.Application.Shared.Cache.Abstractions;
using TrivyOperator.Dashboard.Domain.CveEnrichment.Entities;
using TrivyOperator.Dashboard.Domain.CveEnrichment.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

namespace TrivyOperator.Dashboard.Application.CveEnrichment.Services;

public sealed class KevTimedHostedService(
    IKevProvider provider,
    IOptionsMonitor<CveEnrichmentOptions> options,
    ICveEnrichmentRefreshTrigger trigger,
    ICache<CveId, Timestamp> subjects,
    ICache<CveId, CveInfo> infos,
    ILogger<KevTimedHostedService> logger
) : CveCatalogTimedHostedService<KevEntry>(trigger, subjects, infos, logger)
{
    protected override CveSource Source => CveSources.Kev;
    protected override TimeSpan ProcessingInterval => FromSeconds(options.CurrentValue.Kev.ProcessingIntervalInSeconds);
    protected override TimeSpan RefreshInterval => FromMinutes(options.CurrentValue.Kev.RefreshIntervalInMinutes);
    protected override TimeSpan ErrorTtl => FromMinutes(options.CurrentValue.Kev.ErrorTtlInMinutes);

    protected override Task<CveProviderResult<KevEntry>> FetchCatalog(string? validator, CancellationToken ctx) =>
        provider.GetCatalog(options.CurrentValue.Kev.Url, validator, ctx);

    protected override CveId GetId(KevEntry entry) => entry.Id;

    protected override CveSourceResult<KevEntry>? GetPart(CveInfo info) => info.Kev;

    protected override CveInfo WithPart(CveInfo info, CveSourceResult<KevEntry> part) => info with { Kev = part, };
}
