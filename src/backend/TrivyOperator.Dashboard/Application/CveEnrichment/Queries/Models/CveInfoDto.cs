using TrivyOperator.Dashboard.Domain.CveEnrichment.Entities;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

namespace TrivyOperator.Dashboard.Application.CveEnrichment.Queries.Models;

// LastSeen - newest scan timestamp of the reports containing the CVE
// a null source part means the source did not process the CVE yet (or the source is disabled)
public sealed record CveInfoDto(
    string CveId,
    DateTime LastSeen,
    KevResultDto? Kev,
    EpssResultDto? Epss,
    EuvdResultDto? Euvd);

public sealed record KevResultDto(
    string Status,
    string? ErrorMessage,
    DateTime FetchedAt,
    DateOnly? DateAdded,
    DateOnly? DueDate,
    bool? KnownRansomwareCampaignUse,
    string? RequiredAction);

public sealed record EpssResultDto(
    string Status,
    string? ErrorMessage,
    DateTime FetchedAt,
    double? Score,
    double? Percentile,
    DateOnly? Date);

public sealed record EuvdResultDto(
    string Status,
    string? ErrorMessage,
    DateTime FetchedAt,
    string? EuvdId,
    DateTime? ExploitedSince,
    double? Epss);

public static class CveInfoDtoExtensions
{
    public static CveInfoDto ToCveInfoDto(this CveInfo? info, CveId id, Timestamp lastSeen) =>
        new(
            CveId: id.Value,
            LastSeen: lastSeen.Value,
            Kev: info?.Kev?.ToKevResultDto(),
            Epss: info?.Epss?.ToEpssResultDto(),
            Euvd: info?.Euvd?.ToEuvdResultDto());

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
