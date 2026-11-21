using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.CveEnrichment.Queries.Services.Abstractions;
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
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
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
    ICveEnrichmentService cveEnrichmentService,
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
        HistoryDrift drift = await CollectHistoryDrift(days, filter, ctx);

        if (drift.Reason is not null)
        {
            return new HomeHistoryDriftDto(false, drift.Reason, drift.From, []);
        }

        // distinct (vulnerability id, severity) pairs per day they were added to any image: a pair counts on every day it
        // was added, also when it was added on an earlier day to other images
        Dictionary<DateOnly, List<(int SeverityId, bool IsKnownExploited)>> byDay = drift.Entries
            .GroupBy(x => (x.Pair, Day: DateOnly.FromDateTime(x.AddedAt)))
            .GroupBy(x => x.Key.Day)
            .ToDictionary(
                x => x.Key,
                x => x.Select(pair => (pair.Key.Pair.SeverityId, drift.ActiveById[pair.Key.Pair.Id].IsKnownExploited)).ToList());

        return new HomeHistoryDriftDto(
            true,
            null,
            drift.From,
            [
                .. EachDay(drift.From).Select(day =>
                {
                    List<(int SeverityId, bool IsKnownExploited)> dayPairs = byDay.GetValueOrDefault(day) ?? [];
                    return new HomeHistoryDriftDayDto(
                        day,
                        dayPairs.Count,
                        ToSeverityCounts(dayPairs.Select(x => x.SeverityId)),
                        dayPairs.Count(x => x.IsKnownExploited));
                }),
            ]);
    }

    // Items: one row per history entry (digest + cves hash) and (vulnerability id, severity) added in it, as the history
    // has them; Groups: one per (vulnerability id, severity)
    public async Task<HomeHistoryDriftDenormalizedDto> GetHistoryDriftDenormalized(
        int? days,
        HomeFilter filter,
        CancellationToken ctx = default)
    {
        HistoryDrift drift = await CollectHistoryDrift(days, filter, ctx);

        if (drift.Reason is not null)
        {
            return new HomeHistoryDriftDenormalizedDto(false, drift.Reason, drift.From, [], []);
        }

        return new HomeHistoryDriftDenormalizedDto(
            true,
            null,
            drift.From,
            [
                .. drift.Entries
                    .Select(entry => new HomeHistoryDriftDenormalizedItemDto(
                        VulnerabilityId: entry.Pair.Id.Value,
                        SeverityId: entry.Pair.SeverityId,
                        AddedAt: entry.AddedAt,
                        ImageDigest: entry.ImageDigest,
                        Registry: entry.Meta.Registry.Value,
                        Repository: entry.Meta.Repo.Value,
                        Tag: entry.Meta.Tag.Value,
                        Count: entry.Count,
                        Namespaces: entry.Namespaces.Count,
                        NamespaceNames: ToNamespaceNames(entry.Namespaces)))
                    .OrderBy(x => x.AddedAt)
                    .ThenBy(x => x.SeverityId)
                    .ThenBy(x => x.VulnerabilityId, StringComparer.Ordinal)
                    .ThenBy(x => x.ImageDigest, StringComparer.Ordinal),
            ],
            [
                .. drift.Entries
                    .GroupBy(x => x.Pair)
                    .Select(group =>
                    {
                        ActiveVulnerability row = drift.ActiveById[group.Key.Id];
                        HashSet<NamespaceName> namespaces = [.. group.SelectMany(x => x.Namespaces),];

                        return new HomeHistoryDriftGroupDto(
                            VulnerabilityId: group.Key.Id.Value,
                            SeverityId: group.Key.SeverityId,
                            Title: row.Title,
                            Count: group.Sum(x => x.Count),
                            FirstAddedAt: group.Min(x => x.AddedAt),
                            IsKnownExploited: row.IsKnownExploited,
                            EpssPercentile: row.EpssPercentile,
                            Namespaces: namespaces.Count,
                            NamespaceNames: ToNamespaceNames(namespaces));
                    })
                    .OrderBy(x => x.FirstAddedAt)
                    .ThenBy(x => x.SeverityId)
                    .ThenBy(x => x.VulnerabilityId, StringComparer.Ordinal),
            ]);
    }

    // a history entry (digest + cves hash) and a (vulnerability id, severity) added in it; Namespaces: the ones of the
    // entry, inside the namespaces filter
    private sealed record DriftEntry(
        (VulnerabilityId Id, int SeverityId) Pair,
        DateTime AddedAt,
        string ImageDigest,
        // the occurrences (package, version, target) of the pair in the entry, as the history counts them
        int Count,
        ImageMeta Meta,
        IReadOnlyCollection<NamespaceName> Namespaces);

    private sealed record HistoryDrift(
        string? Reason,
        DateOnly From,
        Dictionary<VulnerabilityId, ActiveVulnerability> ActiveById,
        List<DriftEntry> Entries);

    private async Task<HistoryDrift> CollectHistoryDrift(int? days, HomeFilter filter, CancellationToken ctx)
    {
        DateOnly from = FromDays(days ?? retentionOptions.Value.KeepDays);
        string? reason = GetUnavailableReason();

        if (reason is not null)
        {
            return new HistoryDrift(reason, from, [], []);
        }

        ActiveVulnerabilityTable table = await GetTable(ctx);
        HomeScope scope = new(table, filter.Namespaces);
        HashSet<int> severities = ToSeverities(filter.SeverityIds);

        // the active vulnerability ids (for the "still active" check, the title and the exploited flag) and the
        // running images
        Dictionary<VulnerabilityId, ActiveVulnerability> activeById = table.Rows
            .Where(scope.Contains)
            .GroupBy(x => x.Key.VulnerabilityId)
            .ToDictionary(x => x.Key, x => x.First());
        HashSet<string> runningImageKeys =
            [.. table.Images.Values.Where(x => x.Digest is not null && scope.Contains(x)).Select(x => x.Key),];

        List<DriftEntry> entries = [];

        foreach (SnapshotCveDeltas snapshot in await vulnerabilityReportsHistoryStore.GetAllCveDeltasAsync(ctx))
        {
            DateTime addedAt = snapshot.IndexEntry.FirstSeenAt.Value;
            string imageDigest = snapshot.IndexEntry.Key.Digest.Value;

            if (DateOnly.FromDateTime(addedAt) < from || !runningImageKeys.Contains(imageDigest))
            {
                continue;
            }

            ImageMeta meta = snapshot.IndexEntry.HistoryMetadata.ImageMeta;
            NamespaceName[] namespaces =
            [
                .. snapshot.IndexEntry.HistoryMetadata.NamespaceNames.Where(x => scope.Contains(x, false)),
            ];

            // a pair once per entry, with its occurrences (the history counts one per package, version and target)
            Dictionary<(VulnerabilityId Id, int SeverityId), int> pairs = [];

            foreach (CveDelta delta in snapshot.CvesDeltas.Added)
            {
                if (!severities.Contains(delta.Severity.Rank) || !activeById.ContainsKey(delta.Id))
                {
                    continue;
                }

                (VulnerabilityId, int) pair = (delta.Id, delta.Severity.Rank);
                pairs[pair] = pairs.GetValueOrDefault(pair) + delta.Count;
            }

            foreach (((VulnerabilityId Id, int SeverityId) pair, int count) in pairs)
            {
                entries.Add(new DriftEntry(pair, addedAt, imageDigest, count, meta, namespaces));
            }
        }

        return new HistoryDrift(null, from, activeById, entries);
    }

    // ------------------------------------------------------------------------
    // new images
    // ------------------------------------------------------------------------

    public async Task<HomeNewImagesDto> GetNewImages(int? days, HomeFilter filter, CancellationToken ctx = default)
    {
        NewImages data = await CollectNewImages(days, filter, ctx);

        if (data.Reason is not null)
        {
            return new HomeNewImagesDto(false, data.Reason, data.From, null, []);
        }

        Dictionary<DateOnly, HomeNewImagesDayDto> byDay = data.Images
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
            data.From,
            data.HistoryStartedAt,
            [.. EachDay(data.From).Select(day => byDay.GetValueOrDefault(day) ?? new HomeNewImagesDayDto(day, 0, 0, NoSeverityCounts, 0)),]);
    }

    // Items: one row per new image and (vulnerability id, severity) it has now; Groups: one per (vulnerability id, severity)
    public async Task<HomeNewImagesDenormalizedDto> GetNewImagesDenormalized(
        int? days,
        HomeFilter filter,
        CancellationToken ctx = default)
    {
        NewImages data = await CollectNewImages(days, filter, ctx);

        if (data.Reason is not null)
        {
            return new HomeNewImagesDenormalizedDto(false, data.Reason, data.From, [], []);
        }

        // per image and pair: the occurrences (package, version, target); an image without pairs has no rows
        List<NewImageEntry> entries =
        [
            .. data.Images.SelectMany(x =>
            {
                HomeReach reach = data.Scope!.GetReach([x.Image.Key,]);
                IReadOnlyList<string> namespaceNames = ToNamespaceNames(reach.Namespaces);

                return x.Rows
                    .GroupBy(row => (row.Key.VulnerabilityId, SeverityId: row.Severity.Rank))
                    .Select(group => new NewImageEntry(
                        group.Key,
                        x.Image,
                        group.Count(),
                        reach.Workloads.Count,
                        reach.Namespaces,
                        namespaceNames));
            }),
        ];

        return new HomeNewImagesDenormalizedDto(
            true,
            null,
            data.From,
            [
                .. entries
                    .Select(entry => new HomeNewImagesDenormalizedItemDto(
                        VulnerabilityId: entry.Pair.Id.Value,
                        SeverityId: entry.Pair.SeverityId,
                        FirstSeenAt: entry.Image.FirstSeenAt!.Value.Value,
                        ImageDigest: entry.Image.Digest!.Value.Value,
                        Registry: entry.Image.ImageMeta.Registry.Value,
                        Repository: entry.Image.ImageMeta.Repo.Value,
                        Tag: entry.Image.ImageMeta.Tag.Value,
                        Count: entry.Count,
                        Workloads: entry.Workloads,
                        Namespaces: entry.Namespaces.Count,
                        NamespaceNames: entry.NamespaceNames))
                    .OrderBy(x => x.FirstSeenAt)
                    .ThenBy(x => x.SeverityId)
                    .ThenBy(x => x.VulnerabilityId, StringComparer.Ordinal)
                    .ThenBy(x => x.ImageDigest, StringComparer.Ordinal),
            ],
            [
                .. entries
                    .GroupBy(x => x.Pair)
                    .Select(group =>
                    {
                        ActiveVulnerability row = data.RowById[group.Key.Id];
                        HashSet<NamespaceName> namespaces = [.. group.SelectMany(x => x.Namespaces),];

                        return new HomeNewImagesGroupDto(
                            VulnerabilityId: group.Key.Id.Value,
                            SeverityId: group.Key.SeverityId,
                            Title: row.Title,
                            Count: group.Sum(x => x.Count),
                            Images: group.Count(),
                            FirstSeenAt: group.Min(x => x.Image.FirstSeenAt!.Value.Value),
                            IsKnownExploited: row.IsKnownExploited,
                            EpssPercentile: row.EpssPercentile,
                            Namespaces: namespaces.Count,
                            NamespaceNames: ToNamespaceNames(namespaces));
                    })
                    .OrderBy(x => x.FirstSeenAt)
                    .ThenBy(x => x.SeverityId)
                    .ThenBy(x => x.VulnerabilityId, StringComparer.Ordinal),
            ]);
    }

    // a new image with its rows now (inside the severities filter): the distinct (id, severity) pairs and the known
    // exploited ids come from them
    private sealed record NewImage(ActiveImage Image, ActiveVulnerability[] Rows)
    {
        public HashSet<(VulnerabilityId Id, int SeverityId)> Pairs { get; } =
            [.. Rows.Select(x => (x.Key.VulnerabilityId, x.Severity.Rank)),];

        public HashSet<VulnerabilityId> Exploited { get; } =
            [.. Rows.Where(x => x.IsKnownExploited).Select(x => x.Key.VulnerabilityId),];
    }

    // an image and a (vulnerability id, severity) it has; Namespaces: the ones of the image, inside the namespaces filter
    private sealed record NewImageEntry(
        (VulnerabilityId Id, int SeverityId) Pair,
        ActiveImage Image,
        int Count,
        int Workloads,
        IReadOnlySet<NamespaceName> Namespaces,
        IReadOnlyList<string> NamespaceNames);

    private sealed record NewImages(
        string? Reason,
        DateOnly From,
        DateTime? HistoryStartedAt,
        HomeScope? Scope,
        // a row per vulnerability id (any of its rows: the title and the exploited flag are of the id)
        Dictionary<VulnerabilityId, ActiveVulnerability> RowById,
        List<NewImage> Images);

    private async Task<NewImages> CollectNewImages(int? days, HomeFilter filter, CancellationToken ctx)
    {
        DateOnly from = FromDays(days ?? retentionOptions.Value.KeepDays);
        string? reason = GetUnavailableReason();

        if (reason is not null)
        {
            return new NewImages(reason, from, null, null, [], []);
        }

        ActiveVulnerabilityTable table = await GetTable(ctx);
        HomeScope scope = new(table, filter.Namespaces);
        HashSet<int> severities = ToSeverities(filter.SeverityIds);

        Dictionary<PackageVulnerabilityKey, ActiveVulnerability> rows = table.Rows.ToDictionary(x => x.Key);

        // the images first seen in the range, with their rows now; an image without rows (inside the severities filter)
        // is not a new image of this view: neither the days nor the rows have it
        List<NewImage> images =
        [
            .. table.Images.Values
                .Where(x => x.Digest is not null && x.FirstSeenAt is { } firstSeenAt &&
                            DateOnly.FromDateTime(firstSeenAt.Value) >= from && scope.Contains(x))
                .Select(image => new NewImage(
                    image,
                    [
                        .. image.Keys
                            .Select(key => rows.GetValueOrDefault(key))
                            .OfType<ActiveVulnerability>()
                            .Where(x => severities.Contains(x.Severity.Rank)),
                    ]))
                .Where(x => x.Rows.Length > 0),
        ];

        Dictionary<VulnerabilityId, ActiveVulnerability> rowById = images
            .SelectMany(x => x.Rows)
            .GroupBy(x => x.Key.VulnerabilityId)
            .ToDictionary(x => x.Key, x => x.First());

        return new NewImages(null, from, table.HistoryStartedAt?.Value, scope, rowById, images);
    }

    // ------------------------------------------------------------------------
    // helpers
    // ------------------------------------------------------------------------

    // ------------------------------------------------------------------------
    // summaries of the changes widgets: the rows of the new vulnerabilities / new fixes lists, counted per day
    // ------------------------------------------------------------------------

    // the same rule as the list of new vulnerabilities (the first seen of the row), by the UTC day
    public async Task<HomeNewVulnerabilitiesSummaryDto> GetNewVulnerabilitiesSummary(
        int? days,
        HomeFilter filter,
        CancellationToken ctx = default)
    {
        (string? reason, DateOnly from, IReadOnlyList<HomeDaySummaryDto> daySummaries) = await SummarizeRows(
            days,
            filter,
            row => row.FirstAt?.Value,
            ctx);

        return new HomeNewVulnerabilitiesSummaryDto(reason is null, reason, from, daySummaries);
    }

    // the same rule as the list of new fixes (the fix appeared, not for a row that came with its fix), by the UTC day
    public async Task<HomeNewFixesSummaryDto> GetNewFixesSummary(
        int? days,
        HomeFilter filter,
        CancellationToken ctx = default)
    {
        (string? reason, DateOnly from, IReadOnlyList<HomeDaySummaryDto> daySummaries) = await SummarizeRows(
            days,
            filter,
            row => row.HasFix ? row.FixAppearedAt?.Value : null,
            ctx);

        return new HomeNewFixesSummaryDto(reason is null, reason, from, daySummaries);
    }

    private async Task<(string? Reason, DateOnly From, IReadOnlyList<HomeDaySummaryDto> Days)> SummarizeRows(
        int? days,
        HomeFilter filter,
        Func<ActiveVulnerability, DateTime?> getMoment,
        CancellationToken ctx)
    {
        DateOnly from = FromDays(days ?? retentionOptions.Value.KeepDays);
        string? reason = GetUnavailableReason();

        if (reason is not null)
        {
            return (reason, from, []);
        }

        ActiveVulnerabilityTable table = await GetTable(ctx);
        HomeScope scope = new(table, filter.Namespaces);
        HashSet<int> severities = ToSeverities(filter.SeverityIds);

        Dictionary<DateOnly, List<ActiveVulnerability>> byDay = table.Rows
            .Where(row => severities.Contains(row.Severity.Rank) && scope.Contains(row))
            .Select(row => (Row: row, Moment: getMoment(row)))
            .Where(x => x.Moment is not null && DateOnly.FromDateTime(x.Moment.Value) >= from)
            .GroupBy(x => DateOnly.FromDateTime(x.Moment!.Value))
            .ToDictionary(x => x.Key, x => x.Select(y => y.Row).ToList());

        return (
            null,
            from,
            [
                .. EachDay(from).Select(day =>
                {
                    List<ActiveVulnerability> rows = byDay.GetValueOrDefault(day) ?? [];
                    return new HomeDaySummaryDto(day, rows.Count, rows.Select(x => x.Key.VulnerabilityId).Distinct().Count());
                }),
            ]);
    }

    // the ids that became known exploited per day (the moment of the widget's "new"), as in the known exploited list
    public async Task<HomeKnownExploitedSummaryDto> GetKnownExploitedSummary(
        int? days,
        HomeFilter filter,
        CancellationToken ctx = default)
    {
        DateOnly from = FromDays(days ?? retentionOptions.Value.KeepDays);
        string? reason = !sources.HasVulnerabilityReports ? NoVulnerabilityReports
            : !cveEnrichmentService.IsEnabled ? "CVE enrichment is not enabled"
            : null;

        if (reason is not null)
        {
            return new HomeKnownExploitedSummaryDto(false, reason, from, []);
        }

        ActiveVulnerabilityTable table = await GetTable(ctx);
        HomeScope scope = new(table, filter.Namespaces);

        // no severity filter: being exploited matters whatever the severity
        Dictionary<DateOnly, List<ActiveVulnerability>> byDay = table.Rows
            .Where(row => row.IsKnownExploited && scope.Contains(row))
            .Select(row => (Row: row, Since: row.KnownExploitedSince))
            .Where(x => x.Since is not null && DateOnly.FromDateTime(x.Since.Value) >= from)
            .GroupBy(x => DateOnly.FromDateTime(x.Since!.Value))
            .ToDictionary(x => x.Key, x => x.Select(y => y.Row).ToList());

        return new HomeKnownExploitedSummaryDto(
            true,
            null,
            from,
            [
                .. EachDay(from).Select(day =>
                {
                    List<ActiveVulnerability> rows = byDay.GetValueOrDefault(day) ?? [];
                    return new HomeDaySummaryDto(day, rows.Count, rows.Select(x => x.Key.VulnerabilityId).Distinct().Count());
                }),
            ]);
    }

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
