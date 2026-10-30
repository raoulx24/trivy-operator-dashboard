using TrivyOperator.Dashboard.Domain.CveEnrichment.Entities;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

namespace TrivyOperator.Dashboard.Application.CveEnrichment.Queries.Models;

// LastSeen - newest scan timestamp of the reports containing the CVE
// a source part is never null: when the source did not process the CVE yet (or is disabled) it has the NotProcessed status,
// no data and no FetchedAt
public sealed record CveInfoDto(
    string CveId,
    DateTime LastSeen,
    KevResultDto Kev,
    EpssResultDto Epss,
    EuvdResultDto Euvd);

public sealed record KevResultDto(
    string Status,
    string? ErrorMessage,
    DateTime? FetchedAt,
    DateOnly? DateAdded,
    DateOnly? DueDate,
    bool? KnownRansomwareCampaignUse,
    string? RequiredAction)
{
    public static readonly KevResultDto NotProcessed = new(CveInfoDtoStatuses.NotProcessed, null, null, null, null, null, null);
}

public sealed record EpssResultDto(
    string Status,
    string? ErrorMessage,
    DateTime? FetchedAt,
    double? Score,
    double? Percentile,
    DateOnly? Date)
{
    public static readonly EpssResultDto NotProcessed = new(CveInfoDtoStatuses.NotProcessed, null, null, null, null, null);
}

public sealed record EuvdResultDto(
    string Status,
    string? ErrorMessage,
    DateTime? FetchedAt,
    string? EuvdId,
    DateTime? ExploitedSince,
    double? Epss)
{
    public static readonly EuvdResultDto NotProcessed = new(CveInfoDtoStatuses.NotProcessed, null, null, null, null, null);
}

// the statuses of the domain (Ok, NotListed, Error) plus the one only the DTO knows
public static class CveInfoDtoStatuses
{
    public const string NotProcessed = "NotProcessed";
}

public static class CveInfoDtoExtensions
{
    public static CveInfoDto ToCveInfoDto(this CveInfo? info, CveId id, Timestamp lastSeen) =>
        new(
            CveId: id.Value,
            LastSeen: lastSeen.Value,
            Kev: info?.Kev?.ToKevResultDto() ?? KevResultDto.NotProcessed,
            Epss: info?.Epss?.ToEpssResultDto() ?? EpssResultDto.NotProcessed,
            Euvd: info?.Euvd?.ToEuvdResultDto() ?? EuvdResultDto.NotProcessed);

    public static KevResultDto ToKevResultDto(this CveSourceResult<KevEntry> result) =>
        new(
            Status: result.Status.Value,
            ErrorMessage: result.ErrorMessage,
            FetchedAt: result.FetchedAt.Value,
            DateAdded: result.Data?.DateAdded,
            DueDate: result.Data?.DueDate,
            KnownRansomwareCampaignUse: result.Data?.KnownRansomwareCampaignUse,
            RequiredAction: result.Data?.RequiredAction);

    public static EpssResultDto ToEpssResultDto(this CveSourceResult<EpssScore> result) =>
        new(
            Status: result.Status.Value,
            ErrorMessage: result.ErrorMessage,
            FetchedAt: result.FetchedAt.Value,
            Score: result.Data?.Score,
            Percentile: result.Data?.Percentile,
            Date: result.Data?.Date);

    public static EuvdResultDto ToEuvdResultDto(this CveSourceResult<EuvdEntry> result) =>
        new(
            Status: result.Status.Value,
            ErrorMessage: result.ErrorMessage,
            FetchedAt: result.FetchedAt.Value,
            EuvdId: result.Data?.EuvdId.Value,
            ExploitedSince: result.Data?.ExploitedSince?.Value,
            Epss: result.Data?.Epss);
}
