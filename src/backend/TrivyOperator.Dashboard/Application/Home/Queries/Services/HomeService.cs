using TrivyOperator.Dashboard.Application.CveEnrichment.Queries.Models;
using TrivyOperator.Dashboard.Application.CveEnrichment.Queries.Services.Abstractions;
using TrivyOperator.Dashboard.Application.EndOfLife.Queries.Models;
using TrivyOperator.Dashboard.Application.EndOfLife.Queries.Services.Abstractions;
using TrivyOperator.Dashboard.Application.History.Queries.History.Services.Abstractions;
using TrivyOperator.Dashboard.Application.Home.Models;
using TrivyOperator.Dashboard.Application.Home.Queries.Models;
using TrivyOperator.Dashboard.Application.Home.Queries.Services.Abstractions;
using TrivyOperator.Dashboard.Application.Home.Services;
using static TrivyOperator.Dashboard.Application.Home.Services.HomeNames;
using TrivyOperator.Dashboard.Application.Kubernetes.Queries.WatcherStates.Models;
using TrivyOperator.Dashboard.Application.Kubernetes.Queries.WatcherStates.Services.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.WatcherState.Models;
using TrivyOperator.Dashboard.Application.Shared.Queries.Services;
using TrivyOperator.Dashboard.Domain.EndOfLife.ValueObjects;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.SecurityAssessments;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Vulnerabilities;

namespace TrivyOperator.Dashboard.Application.Home.Queries.Services;

public class HomeService(
    HomeReportSources sources,
    ActiveVulnerabilityTableCache tableCache,
    ActiveVulnerabilityTableBuilder tableBuilder,
    HomeAssessmentReportsLoader assessmentReportsLoader,
    IVulnerabilityReportsHistoryService vulnerabilityReportsHistoryService,
    ICveEnrichmentService cveEnrichmentService,
    IEndOfLifeService endOfLifeService,
    IWatcherStatusService watcherStatusService
) : QueryService, IHomeService
{
    private const string NoVulnerabilityReports = "Vulnerability reports are not enabled";
    private const string NoHistory = "Vulnerability reports history is not enabled";
    private const string NoCveEnrichment = "CVE enrichment is not enabled";
    private const string NoEndOfLife = "End of life is not enabled";
    private const string NoSecurityAssessments = "No security assessment reports (config audit, RBAC, infra) are enabled";

    private static readonly int[] DefaultSeverityIds = [Severities.Critical.Rank, Severities.High.Rank,];

    // ------------------------------------------------------------------------
    // Data health
    // ------------------------------------------------------------------------

    public async Task<HomeDataHealthDto> GetDataHealth(CancellationToken ctx = default)
    {
        List<WatcherStatusDto> watchers = [.. await watcherStatusService.GetWatcherStatusDtos(),];
        List<WatcherStatusDto> watcherIssues =
            [.. watchers.Where(x => !string.Equals(x.Status, nameof(WatcherStateStatus.Green), StringComparison.Ordinal)),];

        List<HomeReportFreshnessDto> reports = [];

        if (sources.VulnerabilityReports is not null)
        {
            reports.Add(ToFreshness(nameof(VulnerabilityReport),
                [.. (await sources.VulnerabilityReports.GetResources(ctx)).Select(x => x.LastSeenAt),]));
        }

        if (sources.ClusterVulnerabilityReports is not null)
        {
            reports.Add(ToFreshness(nameof(ClusterVulnerabilityReport),
                [.. (await sources.ClusterVulnerabilityReports.GetResources(ctx)).Select(x => x.LastSeenAt),]));
        }

        foreach (IGrouping<string, HomeAssessmentReport> kind in (await assessmentReportsLoader.Load(ctx)).GroupBy(x => x.Kind))
        {
            reports.Add(ToFreshness(kind.Key, [.. kind.Select(x => x.LastSeenAt),]));
        }

        List<HomeSourceStatusDto> sourceStatuses = [];

        IReadOnlyList<CveInfoDto> cveInfos = cveEnrichmentService.IsEnabled
            ? await cveEnrichmentService.GetAll(ctx)
            : [];
        sourceStatuses.Add(ToSourceStatus("KEV", cveEnrichmentService.IsEnabled, [.. cveInfos.Select(x => (x.Kev.Status, x.Kev.FetchedAt)),]));
        sourceStatuses.Add(ToSourceStatus("EPSS", cveEnrichmentService.IsEnabled, [.. cveInfos.Select(x => (x.Epss.Status, x.Epss.FetchedAt)),]));
        sourceStatuses.Add(ToSourceStatus("EUVD", cveEnrichmentService.IsEnabled, [.. cveInfos.Select(x => (x.Euvd.Status, x.Euvd.FetchedAt)),]));

        IReadOnlyList<EolInfoDto> eolInfos = endOfLifeService.IsEnabled
            ? await endOfLifeService.GetAll(EolSubjectKind.Os, ctx)
            : [];
        sourceStatuses.Add(new HomeSourceStatusDto(
            "EOL",
            endOfLifeService.IsEnabled,
            eolInfos.Count == 0 ? null : eolInfos.Max(x => x.ResolvedAt),
            eolInfos.Count(x => x.Status == EolStatus.Error)));

        ActiveVulnerabilityTable? table = sources.HasVulnerabilityReports ? await GetTable(ctx) : null;

        return new HomeDataHealthDto(
            WatchersTotal: watchers.Count,
            WatchersOk: watchers.Count - watcherIssues.Count,
            WatcherIssues:
            [
                .. watcherIssues.Select(x => new HomeWatcherIssueDto(
                    x.KubernetesObjectType,
                    string.IsNullOrEmpty(x.NamespaceName) ? null : x.NamespaceName,
                    x.Status,
                    x.MitigationMessage)),
            ],
            Reports: reports,
            Sources: sourceStatuses,
            IsVrHistoryEnabled: vulnerabilityReportsHistoryService.IsEnabled,
            HistoryStartedAt: table?.HistoryStartedAt?.Value,
            FixTrackingStartedAt: table?.FixTrackingStartedAt?.Value,
            GeneratedAt: DateTime.UtcNow);
    }

    public async Task<IReadOnlyList<string>> GetNamespaces(CancellationToken ctx = default)
    {
        HashSet<NamespaceName> namespaces = [];

        if (sources.HasVulnerabilityReports)
        {
            foreach (ActiveImage image in (await GetTable(ctx)).Images.Values)
            {
                namespaces.UnionWith(image.WorkloadsByNamespace.Keys);
            }
        }

        foreach (HomeAssessmentReport report in await assessmentReportsLoader.Load(ctx))
        {
            if (!report.IsClusterScoped)
            {
                namespaces.Add(report.NamespaceName);
            }
        }

        return [.. namespaces.Where(x => !x.IsClusterScoped).Select(x => x.Value).Order(StringComparer.Ordinal),];
    }

    private static HomeReportFreshnessDto ToFreshness(string kind, IReadOnlyList<Timestamp> lastSeen) =>
        new(
            kind,
            lastSeen.Count,
            lastSeen.Count == 0 ? null : lastSeen.Min().Value,
            lastSeen.Count == 0 ? null : lastSeen.Max().Value);

    private static HomeSourceStatusDto ToSourceStatus(
        string source,
        bool isEnabled,
        IReadOnlyList<(string Status, DateTime? FetchedAt)> parts)
    {
        DateTime[] fetchedAt = [.. parts.Where(x => x.FetchedAt is not null).Select(x => x.FetchedAt!.Value),];

        return new HomeSourceStatusDto(
            source,
            isEnabled,
            fetchedAt.Length == 0 ? null : fetchedAt.Max(),
            parts.Count(x => string.Equals(x.Status, "Error", StringComparison.Ordinal)));
    }

    // ------------------------------------------------------------------------
    // Changes since the last visit
    // ------------------------------------------------------------------------

    public async Task<HomeNewVulnerabilitiesDto> GetNewVulnerabilities(HomeFilter filter, CancellationToken ctx = default)
    {
        string? reason = !sources.HasVulnerabilityReports ? NoVulnerabilityReports
            : !vulnerabilityReportsHistoryService.IsEnabled ? NoHistory
            : null;

        if (reason is not null)
        {
            return new HomeNewVulnerabilitiesDto(false, reason, filter.Since, null, EmptyTotals, []);
        }

        ActiveVulnerabilityTable table = await GetTable(ctx);
        HomeScope scope = new(table, filter.Namespaces);
        HashSet<int> severities = ToSeverities(filter.SeverityIds);

        List<ActiveVulnerability> rows =
        [
            .. table.Rows.Where(x =>
                severities.Contains(x.Severity.Rank) &&
                x.FirstAt is { } firstAt && firstAt.Value >= filter.Since &&
                scope.Contains(x)),
        ];

        return new HomeNewVulnerabilitiesDto(
            true,
            null,
            filter.Since,
            table.FixTrackingStartedAt?.Value,
            ToTotals(rows, scope),
            ToDtos(OrderByReach(rows, scope), scope, table, filter.Limit));
    }

    public async Task<HomeNewFixesDto> GetNewFixes(HomeFilter filter, CancellationToken ctx = default)
    {
        string? reason = !sources.HasVulnerabilityReports ? NoVulnerabilityReports
            : !vulnerabilityReportsHistoryService.IsEnabled ? NoHistory
            : null;

        if (reason is not null)
        {
            return new HomeNewFixesDto(false, reason, filter.Since, null, EmptyTotals, []);
        }

        ActiveVulnerabilityTable table = await GetTable(ctx);
        HomeScope scope = new(table, filter.Namespaces);
        HashSet<int> severities = ToSeverities(filter.SeverityIds);

        List<ActiveVulnerability> rows =
        [
            .. table.Rows.Where(x =>
                severities.Contains(x.Severity.Rank) && x.HasFix &&
                x.FixAppearedAt is { } fixAppearedAt && fixAppearedAt.Value >= filter.Since &&
                scope.Contains(x)),
        ];

        return new HomeNewFixesDto(
            true,
            null,
            filter.Since,
            table.FixTrackingStartedAt?.Value,
            ToTotals(rows, scope),
            ToDtos(OrderByReach(rows, scope), scope, table, filter.Limit));
    }

    public async Task<HomeKnownExploitedDto> GetKnownExploited(HomeFilter filter, CancellationToken ctx = default)
    {
        string? reason = !sources.HasVulnerabilityReports ? NoVulnerabilityReports
            : !cveEnrichmentService.IsEnabled ? NoCveEnrichment
            : null;

        if (reason is not null)
        {
            return new HomeKnownExploitedDto(false, reason, filter.Since, 0, EmptyTotals, []);
        }

        ActiveVulnerabilityTable table = await GetTable(ctx);
        HomeScope scope = new(table, filter.Namespaces);

        // no severity filter: being exploited matters whatever the severity
        List<ActiveVulnerability> rows = [.. table.Rows.Where(x => x.IsKnownExploited && scope.Contains(x)),];

        List<HomeKnownExploitedItemDto> items =
        [
            .. rows
                .GroupBy(x => x.Key.VulnerabilityId)
                .Select(group =>
                {
                    ActiveVulnerability first = group.First();
                    HomeReach reach = scope.GetReach(group.SelectMany(x => x.ImageKeys));
                    DateTime? exploitedSince = first.KnownExploitedSince;

                    return new HomeKnownExploitedItemDto(
                        VulnerabilityId: group.Key.Value,
                        SeverityId: group.Min(x => x.Severity.Rank),
                        PackageNames: [.. group.Select(x => x.PackageName.Value).Distinct().Order(StringComparer.Ordinal),],
                        KevDateAdded: first.KevDateAdded,
                        KevKnownRansomwareCampaignUse: first.Enrichment?.Kev.KnownRansomwareCampaignUse,
                        EuvdExploitedSince: first.EuvdExploitedSince,
                        EpssPercentile: first.EpssPercentile,
                        IsNewSince: exploitedSince is not null && exploitedSince.Value >= filter.Since,
                        HasFix: group.Any(x => x.HasFix),
                        Images: reach.ImageKeys.Count,
                        Namespaces: reach.Namespaces.Count,
                        Workloads: reach.Workloads.Count,
                        NamespaceNames: ToNamespaceNames(reach.Namespaces),
                        ImageNames: ToImageNames(reach.ImageKeys, table));
                })
                .OrderByDescending(x => x.IsNewSince)
                .ThenByDescending(x => x.Images)
                .ThenBy(x => x.SeverityId),
        ];

        return new HomeKnownExploitedDto(
            true,
            null,
            filter.Since,
            items.Count(x => x.IsNewSince),
            ToTotals(rows, scope),
            [.. items.Take(filter.Limit),]);
    }

    // ------------------------------------------------------------------------
    // Current state
    // ------------------------------------------------------------------------

    public async Task<HomeBaseOsDto> GetBaseOs(HomeFilter filter, CancellationToken ctx = default)
    {
        string? reason = !sources.HasVulnerabilityReports ? NoVulnerabilityReports
            : !endOfLifeService.IsEnabled ? NoEndOfLife
            : null;

        if (reason is not null)
        {
            return new HomeBaseOsDto(false, reason, []);
        }

        ActiveVulnerabilityTable table = await GetTable(ctx);
        HomeScope scope = new(table, filter.Namespaces);

        // same normalization as EolSubject: family trimmed and lower case, version trimmed
        Dictionary<(string Family, string Version), EolInfoDto> eolInfos = (await endOfLifeService.GetAll(EolSubjectKind.Os, ctx))
            .GroupBy(x => (x.Name, x.Version))
            .ToDictionary(x => x.Key, x => x.First());

        List<HomeBaseOsItemDto> items =
        [
            .. table.Images.Values
                .Where(x => x.Os.Family.IsValid && x.Os.Name.IsValid && scope.Contains(x))
                .GroupBy(x => (Family: x.Os.Family.Value.Trim().ToLowerInvariant(), Version: x.Os.Name.Value.Trim()))
                .Select(group =>
                {
                    EolInfoDto? info = eolInfos.GetValueOrDefault(group.Key);
                    EolReleaseDto? release = info?.Release;
                    HomeReach reach = scope.GetReach(group.Select(x => x.Key));

                    return new HomeBaseOsItemDto(
                        Family: group.Key.Family,
                        Version: group.Key.Version,
                        Status: (info?.Status ?? EolStatus.Pending).ToString(),
                        NotApplicableReason: info?.NotApplicableReason?.ToString(),
                        ProductLabel: info?.ProductLabel,
                        Cycle: release?.Cycle,
                        IsEol: release?.IsEol,
                        EolFrom: release?.EolFrom,
                        IsEoas: release?.IsEoas,
                        EoasFrom: release?.EoasFrom,
                        LatestVersion: release?.LatestVersion,
                        Images: reach.ImageKeys.Count,
                        Namespaces: reach.Namespaces.Count,
                        Workloads: reach.Workloads.Count);
                })
                // end of life first, then by the date it ends; the ones without a date last
                .OrderByDescending(x => x.IsEol == true)
                .ThenBy(x => x.EolFrom is null)
                .ThenBy(x => x.EolFrom)
                .ThenBy(x => x.Family, StringComparer.Ordinal)
                .ThenBy(x => x.Version, StringComparer.Ordinal),
        ];

        return new HomeBaseOsDto(true, null, items);
    }

    public async Task<HomeFixWaitingDto> GetFixWaiting(HomeFilter filter, CancellationToken ctx = default)
    {
        if (!sources.HasVulnerabilityReports)
        {
            return new HomeFixWaitingDto(false, NoVulnerabilityReports, null, EmptyTotals, []);
        }

        ActiveVulnerabilityTable table = await GetTable(ctx);
        HomeScope scope = new(table, filter.Namespaces);
        HashSet<int> severities = ToSeverities(filter.SeverityIds);

        List<ActiveVulnerability> rows =
            [.. table.Rows.Where(x => severities.Contains(x.Severity.Rank) && x.HasFix && scope.Contains(x)),];

        // the longest waiting first; without history (no FixSince) by reach only
        IEnumerable<ActiveVulnerability> ordered = rows
            .Select(x => (Row: x, Images: scope.GetReach(x).ImageKeys.Count))
            .OrderBy(x => x.Row.FixSince is null)
            .ThenBy(x => x.Row.FixSince)
            .ThenByDescending(x => x.Images)
            .ThenBy(x => x.Row.Severity.Rank)
            .Select(x => x.Row);

        return new HomeFixWaitingDto(
            true,
            null,
            table.FixTrackingStartedAt?.Value,
            ToTotals(rows, scope),
            ToDtos(ordered, scope, table, filter.Limit));
    }

    public async Task<HomeFailingChecksDto> GetFailingChecks(HomeFilter filter, CancellationToken ctx = default)
    {
        if (!sources.HasSecurityAssessmentReports)
        {
            return new HomeFailingChecksDto(false, NoSecurityAssessments, 0, []);
        }

        // the scope works on reports here, the table is not needed
        HomeScope scope = new(EmptyTable, filter.Namespaces);
        HashSet<int> severities = ToSeverities(filter.SeverityIds);

        List<HomeFailingCheckDto> items =
        [
            .. (await assessmentReportsLoader.Load(ctx))
                .Where(report => scope.Contains(report.NamespaceName, report.IsClusterScoped))
                .SelectMany(report => report.Checks.Select(check => (Report: report, Check: check)))
                .Where(x => severities.Contains(x.Check.Severity.Rank))
                .GroupBy(x => (x.Report.Family, x.Check.CheckId))
                .Select(group =>
                {
                    Check check = group.First().Check;

                    return new HomeFailingCheckDto(
                        Family: group.Key.Family,
                        CheckId: group.Key.CheckId.Value,
                        Title: check.Title.Value,
                        SeverityId: check.Severity.Rank,
                        Resources: group.Select(x => x.Report.Uid).Distinct().Count(),
                        Namespaces: group
                            .Where(x => !x.Report.IsClusterScoped)
                            .Select(x => x.Report.NamespaceName)
                            .Distinct()
                            .Count(),
                        HasClusterScopedResources: group.Any(x => x.Report.IsClusterScoped));
                })
                .OrderBy(x => x.SeverityId)
                .ThenByDescending(x => x.Resources)
                .ThenBy(x => x.CheckId, StringComparer.Ordinal),
        ];

        return new HomeFailingChecksDto(true, null, items.Count, [.. items.Take(filter.Limit),]);
    }

    public async Task<HomeOldestImagesDto> GetOldestImages(HomeFilter filter, CancellationToken ctx = default)
    {
        string? reason = sources.VulnerabilityReports is null ? NoVulnerabilityReports
            : !vulnerabilityReportsHistoryService.IsEnabled ? NoHistory
            : null;

        if (reason is not null)
        {
            return new HomeOldestImagesDto(false, reason, null, []);
        }

        ActiveVulnerabilityTable table = await GetTable(ctx);
        HomeScope scope = new(table, filter.Namespaces);
        HashSet<int> severities = ToSeverities(filter.SeverityIds);
        Dictionary<PackageVulnerabilityKey, ActiveVulnerability> rows =
            table.Rows.ToDictionary(x => x.Key);

        List<HomeOldestImageDto> items =
        [
            .. table.Images.Values
                .Where(x => x.Digest is not null && x.FirstSeenAt is not null && scope.Contains(x))
                .OrderBy(x => x.FirstSeenAt)
                .Take(filter.Limit)
                .Select(image =>
                {
                    HomeReach reach = scope.GetReach([image.Key,]);

                    return new HomeOldestImageDto(
                        Digest: image.Digest!.Value.Value,
                        Registry: image.ImageMeta.Registry.Value,
                        Repository: image.ImageMeta.Repo.Value,
                        Tag: image.ImageMeta.Tag.Value,
                        FirstSeenAt: image.FirstSeenAt!.Value.Value,
                        Workloads: reach.Workloads.Count,
                        Namespaces: reach.Namespaces.Count,
                        NamespaceNames: ToNamespaceNames(reach.Namespaces),
                        VulnerabilityIds: image.Keys
                            .Where(key => rows.TryGetValue(key, out ActiveVulnerability? row) &&
                                          severities.Contains(row.Severity.Rank))
                            .Select(key => key.VulnerabilityId)
                            .Distinct()
                            .Count());
                }),
        ];

        return new HomeOldestImagesDto(true, null, table.HistoryStartedAt?.Value, items);
    }

    public async Task<HomePackageHotspotsDto> GetPackageHotspots(HomeFilter filter, CancellationToken ctx = default)
    {
        if (!sources.HasVulnerabilityReports)
        {
            return new HomePackageHotspotsDto(false, NoVulnerabilityReports, []);
        }

        ActiveVulnerabilityTable table = await GetTable(ctx);
        HomeScope scope = new(table, filter.Namespaces);
        HashSet<int> severities = ToSeverities(filter.SeverityIds);

        return new HomePackageHotspotsDto(
            true,
            null,
            [
                .. table.Rows
                    .Where(x => severities.Contains(x.Severity.Rank) && scope.Contains(x))
                    .GroupBy(x => x.Key.PackageKey)
                    .Select(group =>
                    {
                        HomeReach reach = scope.GetReach(group.SelectMany(x => x.ImageKeys));

                        return new HomePackageHotspotDto(
                            PackageKey: group.Key.Value,
                            PackageName: group.First().PackageName.Value,
                            VulnerabilityIds: group.Select(x => x.Key.VulnerabilityId).Distinct().Count(),
                            FixableVulnerabilityIds: group.Where(x => x.HasFix).Select(x => x.Key.VulnerabilityId).Distinct().Count(),
                            Images: reach.ImageKeys.Count,
                            Namespaces: reach.Namespaces.Count);
                    })
                    .OrderByDescending(x => x.VulnerabilityIds)
                    .ThenByDescending(x => x.Images)
                    .ThenBy(x => x.PackageKey, StringComparer.Ordinal)
                    .Take(filter.Limit),
            ]);
    }

    public async Task<HomeReachEpssDto> GetReachEpss(HomeFilter filter, CancellationToken ctx = default)
    {
        string? reason = !sources.HasVulnerabilityReports ? NoVulnerabilityReports
            : !cveEnrichmentService.IsEnabled ? NoCveEnrichment
            : null;

        if (reason is not null)
        {
            return new HomeReachEpssDto(false, reason, []);
        }

        ActiveVulnerabilityTable table = await GetTable(ctx);
        HomeScope scope = new(table, filter.Namespaces);
        HashSet<int> severities = ToSeverities(filter.SeverityIds);

        return new HomeReachEpssDto(
            true,
            null,
            [
                .. table.Rows
                    .Where(x => severities.Contains(x.Severity.Rank) && x.EpssPercentile is not null && scope.Contains(x))
                    .GroupBy(x => x.Key.VulnerabilityId)
                    .Select(group =>
                    {
                        HomeReach reach = scope.GetReach(group.SelectMany(x => x.ImageKeys));

                        return new HomeReachEpssItemDto(
                            VulnerabilityId: group.Key.Value,
                            SeverityId: group.Min(x => x.Severity.Rank),
                            EpssPercentile: group.First().EpssPercentile!.Value,
                            IsKnownExploited: group.First().IsKnownExploited,
                            Images: reach.ImageKeys.Count,
                            Namespaces: reach.Namespaces.Count);
                    })
                    .OrderByDescending(x => x.EpssPercentile)
                    .ThenByDescending(x => x.Images)
                    .Take(filter.Limit),
            ]);
    }

    // ------------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------------

    private static readonly HomeTotalsDto EmptyTotals = new(0, 0, 0, 0, 0);

    private static readonly ActiveVulnerabilityTable EmptyTable = new() { Rows = [], Images = new Dictionary<string, ActiveImage>(), };

    private Task<ActiveVulnerabilityTable> GetTable(CancellationToken ctx) =>
        tableCache.GetOrBuild(tableBuilder.Build, ctx);

    private static HashSet<int> ToSeverities(IReadOnlyCollection<int>? severityIds) =>
        severityIds is { Count: > 0, } ? [.. severityIds,] : [.. DefaultSeverityIds,];

    private static IEnumerable<ActiveVulnerability> OrderByReach(IEnumerable<ActiveVulnerability> rows, HomeScope scope) =>
        rows
            .Select(x => (Row: x, Images: scope.GetReach(x).ImageKeys.Count))
            .OrderByDescending(x => x.Images)
            .ThenBy(x => x.Row.Severity.Rank)
            .ThenBy(x => x.Row.Key.VulnerabilityId.Value, StringComparer.Ordinal)
            .Select(x => x.Row);

    private static HomeTotalsDto ToTotals(IReadOnlyCollection<ActiveVulnerability> rows, HomeScope scope)
    {
        HomeReach reach = scope.GetReach(rows.SelectMany(x => x.ImageKeys));

        return new HomeTotalsDto(
            VulnerabilityIds: rows.Select(x => x.Key.VulnerabilityId).Distinct().Count(),
            ActiveVulnerabilities: rows.Count,
            Images: reach.ImageKeys.Count,
            Namespaces: reach.Namespaces.Count,
            Workloads: reach.Workloads.Count);
    }

    private static IReadOnlyList<HomeActiveVulnerabilityDto> ToDtos(
        IEnumerable<ActiveVulnerability> rows,
        HomeScope scope,
        ActiveVulnerabilityTable table,
        int limit) =>
        [.. rows.Take(limit).Select(x => ToDto(x, scope.GetReach(x), table)),];

    private static HomeActiveVulnerabilityDto ToDto(ActiveVulnerability row, HomeReach reach, ActiveVulnerabilityTable table) =>
        new(
            VulnerabilityId: row.Key.VulnerabilityId.Value,
            PackageKey: row.Key.PackageKey.Value,
            PackageName: row.PackageName.Value,
            SeverityId: row.Severity.Rank,
            FixedVersion: row.HasFix ? row.FixedVersion!.Value.Value : null,
            Published: row.Published?.Value,
            FirstSeenAt: row.FirstAt?.Value,
            FixSince: row.FixSince?.Value,
            FixAppearedAt: row.FixAppearedAt?.Value,
            IsKnownExploited: row.IsKnownExploited,
            EpssPercentile: row.EpssPercentile,
            Images: reach.ImageKeys.Count,
            Namespaces: reach.Namespaces.Count,
            Workloads: reach.Workloads.Count,
            NamespaceNames: ToNamespaceNames(reach.Namespaces),
            ImageNames: ToImageNames(reach.ImageKeys, table));
}
