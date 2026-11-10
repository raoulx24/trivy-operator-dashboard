using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.History.Queries.History.Services.Abstractions;
using TrivyOperator.Dashboard.Application.History.VulnerabilityReportsHistory.Retention;
using TrivyOperator.Dashboard.Application.Home.Models;
using TrivyOperator.Dashboard.Application.Home.Queries.Models;
using TrivyOperator.Dashboard.Application.Home.Queries.Services.Abstractions;
using TrivyOperator.Dashboard.Application.Home.Services;
using TrivyOperator.Dashboard.Application.Shared.Queries.Services;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.Entities;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Vulnerabilities;
using static TrivyOperator.Dashboard.Application.Home.Services.HomeNames;

namespace TrivyOperator.Dashboard.Application.Home.Queries.Services;

public class HomeTimelineService(
    HomeReportSources sources,
    ActiveVulnerabilityTableCache tableCache,
    ActiveVulnerabilityTableBuilder tableBuilder,
    IVulnerabilityReportsHistoryStore vulnerabilityReportsHistoryStore,
    IVulnerabilityReportsHistoryService vulnerabilityReportsHistoryService,
    IOptions<RetentionOptions> retentionOptions
) : QueryService, IHomeTimelineService
{
    private const string NoVulnerabilityReports = "Vulnerability reports are not enabled";
    private const string NoHistory = "Vulnerability reports history is not enabled";

    private const int MaxDays = 366;

    private static readonly int[] DefaultSeverityIds = [Severities.Critical.Rank, Severities.High.Rank,];

    // ------------------------------------------------------------------------
    // history drift
    // ------------------------------------------------------------------------

    public async Task<HomeHistoryDriftDto> GetHistoryDrift(int? days, HomeFilter filter, CancellationToken ctx = default)
    {
        DateOnly from = FromDays(days ?? retentionOptions.Value.KeepDays);
        string? reason = GetUnavailableReason();

        if (reason is not null)
        {
            return new HomeHistoryDriftDto(false, reason, from, [], []);
        }

        ActiveVulnerabilityTable table = await GetTable(ctx);
        HomeScope scope = new(table, filter.Namespaces);
        HashSet<int> severities = ToSeverities(filter.SeverityIds);

        // the active vulnerability ids (for the "still active" check and the exploited flag) and the running images
        Dictionary<VulnerabilityId, ActiveVulnerability> activeById = table.Rows
            .Where(scope.Contains)
            .GroupBy(x => x.Key.VulnerabilityId)
            .ToDictionary(x => x.Key, x => x.First());
        HashSet<string> runningImageKeys =
            [.. table.Images.Values.Where(x => x.Digest is not null && scope.Contains(x)).Select(x => x.Key),];

        // (id, severity) -> the first moment it was added in the range, and the images it was added to
        Dictionary<(VulnerabilityId Id, int SeverityId), (DateTime AddedAt, HashSet<string> ImageKeys)> added = [];

        foreach (SnapshotCveDeltas snapshot in await vulnerabilityReportsHistoryStore.GetAllCveDeltasAsync(ctx))
        {
            DateTime addedAt = snapshot.IndexEntry.FirstSeenAt.Value;
            string imageKey = snapshot.IndexEntry.Key.Digest.Value;

            if (DateOnly.FromDateTime(addedAt) < from || !runningImageKeys.Contains(imageKey))
            {
                continue;
            }

            foreach (CveDelta delta in snapshot.CvesDeltas.Added)
            {
                if (!severities.Contains(delta.Severity.Rank) || !activeById.ContainsKey(delta.Id))
                {
                    continue;
                }

                (VulnerabilityId, int) key = (delta.Id, delta.Severity.Rank);

                if (added.TryGetValue(key, out (DateTime AddedAt, HashSet<string> ImageKeys) current))
                {
                    current.ImageKeys.Add(imageKey);

                    if (addedAt < current.AddedAt)
                    {
                        added[key] = (addedAt, current.ImageKeys);
                    }
                }
                else
                {
                    added[key] = (addedAt, [imageKey,]);
                }
            }
        }

        List<HomeHistoryDriftItemDto> items =
        [
            .. added
                .Select(x =>
                {
                    ActiveVulnerability row = activeById[x.Key.Id];

                    return new HomeHistoryDriftItemDto(
                        VulnerabilityId: x.Key.Id.Value,
                        SeverityId: x.Key.SeverityId,
                        AddedAt: x.Value.AddedAt,
                        IsKnownExploited: row.IsKnownExploited,
                        EpssPercentile: row.EpssPercentile,
                        Images: x.Value.ImageKeys.Count,
                        ImageNames: ToImageNames(x.Value.ImageKeys, table));
                })
                .OrderBy(x => x.AddedAt)
                .ThenBy(x => x.SeverityId)
                .ThenBy(x => x.VulnerabilityId, StringComparer.Ordinal),
        ];

        Dictionary<DateOnly, HomeHistoryDriftItemDto[]> byDay = items
            .GroupBy(x => DateOnly.FromDateTime(x.AddedAt))
            .ToDictionary(x => x.Key, x => x.ToArray());

        return new HomeHistoryDriftDto(
            true,
            null,
            from,
            [
                .. EachDay(from).Select(day =>
                {
                    HomeHistoryDriftItemDto[] dayItems = byDay.GetValueOrDefault(day) ?? [];
                    return new HomeHistoryDriftDayDto(
                        day,
                        dayItems.Length,
                        ToSeverityCounts(dayItems.Select(x => x.SeverityId)),
                        dayItems.Count(x => x.IsKnownExploited));
                }),
            ],
            items);
    }

    // ------------------------------------------------------------------------
    // new images
    // ------------------------------------------------------------------------

    public async Task<HomeNewImagesDto> GetNewImages(int? days, HomeFilter filter, CancellationToken ctx = default)
    {
        string? reason = GetUnavailableReason();

        if (reason is not null)
        {
            return new HomeNewImagesDto(false, reason, FromDays(days ?? retentionOptions.Value.KeepDays), null, [], []);
        }

        ActiveVulnerabilityTable table = await GetTable(ctx);
        HomeScope scope = new(table, filter.Namespaces);
        HashSet<int> severities = ToSeverities(filter.SeverityIds);

        // default: since the history start
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        int defaultDays = table.HistoryStartedAt is { } historyStartedAt
            ? today.DayNumber - DateOnly.FromDateTime(historyStartedAt.Value).DayNumber + 1
            : retentionOptions.Value.KeepDays;
        DateOnly from = FromDays(days ?? defaultDays);

        Dictionary<PackageVulnerabilityKey, ActiveVulnerability> rows = table.Rows.ToDictionary(x => x.Key);

        // the images with their distinct (id, severity) pairs and known exploited ids, now
        List<(ActiveImage Image, HashSet<(VulnerabilityId Id, int SeverityId)> Pairs, HashSet<VulnerabilityId> Exploited)> images =
        [
            .. table.Images.Values
                .Where(x => x.Digest is not null && x.FirstSeenAt is { } firstSeenAt &&
                            DateOnly.FromDateTime(firstSeenAt.Value) >= from && scope.Contains(x))
                .Select(image =>
                {
                    ActiveVulnerability[] imageRows =
                    [
                        .. image.Keys
                            .Select(key => rows.GetValueOrDefault(key))
                            .OfType<ActiveVulnerability>()
                            .Where(x => severities.Contains(x.Severity.Rank)),
                    ];

                    return (
                        image,
                        imageRows.Select(x => (x.Key.VulnerabilityId, x.Severity.Rank)).ToHashSet(),
                        imageRows.Where(x => x.IsKnownExploited).Select(x => x.Key.VulnerabilityId).ToHashSet());
                }),
        ];

        Dictionary<DateOnly, HomeNewImagesDayDto> byDay = images
            .GroupBy(x => DateOnly.FromDateTime(x.Image.FirstSeenAt!.Value.Value))
            .ToDictionary(
                x => x.Key,
                x =>
                {
                    (VulnerabilityId Id, int SeverityId)[] pairs = [.. x.SelectMany(y => y.Pairs).Distinct(),];

                    return new HomeNewImagesDayDto(
                        x.Key,
                        x.Count(),
                        pairs.Length,
                        ToSeverityCounts(pairs.Select(y => y.SeverityId)),
                        x.SelectMany(y => y.Exploited).Distinct().Count());
                });

        return new HomeNewImagesDto(
            true,
            null,
            from,
            table.HistoryStartedAt?.Value,
            [.. EachDay(from).Select(day => byDay.GetValueOrDefault(day) ?? new HomeNewImagesDayDto(day, 0, 0, NoSeverityCounts, 0)),],
            [
                .. images
                    .OrderByDescending(x => x.Image.FirstSeenAt)
                    .Take(filter.Limit)
                    .Select(x =>
                    {
                        HomeReach reach = scope.GetReach([x.Image.Key,]);

                        return new HomeNewImageDto(
                            Digest: x.Image.Digest!.Value.Value,
                            Registry: x.Image.ImageMeta.Registry.Value,
                            Repository: x.Image.ImageMeta.Repo.Value,
                            Tag: x.Image.ImageMeta.Tag.Value,
                            FirstSeenAt: x.Image.FirstSeenAt!.Value.Value,
                            Vulnerabilities: x.Pairs.Count,
                            VulnerabilitiesBySeverity: ToSeverityCounts(x.Pairs.Select(y => y.SeverityId)),
                            KnownExploited: x.Exploited.Count,
                            Workloads: reach.Workloads.Count,
                            Namespaces: reach.Namespaces.Count,
                            NamespaceNames: ToNamespaceNames(reach.Namespaces));
                    }),
            ]);
    }

    // ------------------------------------------------------------------------
    // helpers
    // ------------------------------------------------------------------------

    private string? GetUnavailableReason() =>
        !sources.HasVulnerabilityReports ? NoVulnerabilityReports
        : !vulnerabilityReportsHistoryService.IsEnabled ? NoHistory
        : null;

    private Task<ActiveVulnerabilityTable> GetTable(CancellationToken ctx) =>
        tableCache.GetOrBuild(tableBuilder.Build, ctx);

    private static readonly HomeSeverityCountsDto NoSeverityCounts = new(0, 0, 0, 0, 0);

    // severity ids of distinct pairs -> counts per severity (None, if any, is counted with Unknown)
    private static HomeSeverityCountsDto ToSeverityCounts(IEnumerable<int> severityIds)
    {
        int[] counts = new int[5];

        foreach (int severityId in severityIds)
        {
            counts[Math.Clamp(severityId, 0, 4)]++;
        }

        return new HomeSeverityCountsDto(counts[0], counts[1], counts[2], counts[3], counts[4]);
    }

    private static HashSet<int> ToSeverities(IReadOnlyCollection<int>? severityIds) =>
        severityIds is { Count: > 0, } ? [.. severityIds,] : [.. DefaultSeverityIds,];

    // the first day of a range of days ending today (UTC)
    private static DateOnly FromDays(int days) =>
        DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-(Math.Clamp(days, 1, MaxDays) - 1));

    private static IEnumerable<DateOnly> EachDay(DateOnly from)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

        for (DateOnly day = from; day <= today; day = day.AddDays(1))
        {
            yield return day;
        }
    }
}
