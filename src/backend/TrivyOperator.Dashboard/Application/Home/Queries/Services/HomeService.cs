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
    private const string NoCompliance = "Cluster compliance reports are not enabled";
    private const string NoExposedSecrets = "Exposed secret reports are not enabled";
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

        List<ActiveVulnerability> rows = [.. table.Rows.Where(x => IsNew(x, filter.Since, severities, scope)),];

        return new HomeNewVulnerabilitiesDto(
            true,
            null,
            filter.Since,
            table.FixTrackingStartedAt?.Value,
            ToTotals(rows, scope),
            ToDtos(OrderByReach(rows, scope), scope, table, filter.Limit));
    }

    // one id, everything: the rows of the id that are new (the same rule as the list), their images and namespaces
    public async Task<HomeNewVulnerabilityDetailDto> GetNewVulnerabilityDetailed(
        string vulnerabilityId,
        HomeFilter filter,
        CancellationToken ctx = default)
    {
        string id = vulnerabilityId.Trim();

        string? reason = !sources.HasVulnerabilityReports ? NoVulnerabilityReports
            : !vulnerabilityReportsHistoryService.IsEnabled ? NoHistory
            : null;

        if (reason is not null)
        {
            return new HomeNewVulnerabilityDetailDto(false, reason, id, filter.Since, null, null);
        }

        ActiveVulnerabilityTable table = await GetTable(ctx);
        HomeScope scope = new(table, filter.Namespaces);
        HashSet<int> severities = ToSeverities(filter.SeverityIds);

        List<ActiveVulnerability> rows =
        [
            .. table.Rows.Where(x =>
                string.Equals(x.Key.VulnerabilityId.Value, id, StringComparison.OrdinalIgnoreCase) &&
                IsNew(x, filter.Since, severities, scope)),
        ];

        if (rows.Count == 0)
        {
            return new HomeNewVulnerabilityDetailDto(
                true,
                null,
                id,
                filter.Since,
                "Not new in this window, severities and namespaces.",
                null);
        }

        ActiveVulnerability first = rows[0];
        HomeReach reach = scope.GetReach(rows.SelectMany(x => x.ImageKeys));
        HomeImageRelations relations = HomeImageRelations.Build(reach, scope, table);

        HomeNewVulnerabilityDetailItemDto item = new(
            VulnerabilityId: first.Key.VulnerabilityId.Value,
            Title: first.Title,
            SeverityId: rows.Min(x => x.Severity.Rank),
            IsKnownExploited: first.IsKnownExploited,
            KevDateAdded: first.KevDateAdded,
            EpssPercentile: first.EpssPercentile,
            Packages:
            [
                .. rows
                    .Select(row =>
                    {
                        HomeReach rowReach = scope.GetReach(row);

                        return new HomeNewVulnerabilityPackageDto(
                            PackageKey: row.Key.PackageKey.Value,
                            PackageName: row.PackageName.Value,
                            InstalledVersions: HomeImageRelations.InstalledVersionsInScope(row, scope, table),
                            FixedVersion: row.HasFix ? row.FixedVersion!.Value.Value : null,
                            SeverityId: row.Severity.Rank,
                            Published: row.Published?.Value,
                            FirstSeenAt: row.FirstAt!.Value.Value,
                            FixSince: row.FixSince?.Value,
                            FixAppearedAt: row.FixAppearedAt?.Value,
                            Images: rowReach.ImageKeys.Count,
                            ImageIndexes:
                            [
                                .. rowReach.ImageKeys
                                    .Select(key => relations.ImageIndexes[ToImageName(key, table)])
                                    .Distinct()
                                    .Order(),
                            ]);
                    })
                    .OrderBy(x => x.SeverityId)
                    .ThenByDescending(x => x.Images),
            ],
            Images: reach.ImageKeys.Count,
            Namespaces: reach.Namespaces.Count,
            Workloads: reach.Workloads.Count,
            NamespaceNames: relations.NamespaceNames,
            ImageNames: relations.ImageNames,
            ImageNamespaceIndexes: relations.ImageNamespaceIndexes);

        return new HomeNewVulnerabilityDetailDto(true, null, id, filter.Since, null, item);
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

        List<ActiveVulnerability> rows = [.. table.Rows.Where(x => IsNewFix(x, filter.Since, severities, scope)),];

        return new HomeNewFixesDto(
            true,
            null,
            filter.Since,
            table.FixTrackingStartedAt?.Value,
            ToTotals(rows, scope),
            ToDtos(OrderByReach(rows, scope), scope, table, filter.Limit));
    }

    // one id, everything: the rows of the id whose fix is new (the same rule as the list), their images and namespaces
    public async Task<HomeNewFixDetailDto> GetNewFixDetailed(
        string vulnerabilityId,
        HomeFilter filter,
        CancellationToken ctx = default)
    {
        string id = vulnerabilityId.Trim();

        string? reason = !sources.HasVulnerabilityReports ? NoVulnerabilityReports
            : !vulnerabilityReportsHistoryService.IsEnabled ? NoHistory
            : null;

        if (reason is not null)
        {
            return new HomeNewFixDetailDto(false, reason, id, filter.Since, null, null);
        }

        ActiveVulnerabilityTable table = await GetTable(ctx);
        HomeScope scope = new(table, filter.Namespaces);
        HashSet<int> severities = ToSeverities(filter.SeverityIds);

        List<ActiveVulnerability> rows =
        [
            .. table.Rows.Where(x =>
                string.Equals(x.Key.VulnerabilityId.Value, id, StringComparison.OrdinalIgnoreCase) &&
                IsNewFix(x, filter.Since, severities, scope)),
        ];

        if (rows.Count == 0)
        {
            return new HomeNewFixDetailDto(
                true,
                null,
                id,
                filter.Since,
                "No new fix in this window, severities and namespaces.",
                null);
        }

        ActiveVulnerability first = rows[0];
        HomeReach reach = scope.GetReach(rows.SelectMany(x => x.ImageKeys));
        HomeImageRelations relations = HomeImageRelations.Build(reach, scope, table);

        HomeNewFixDetailItemDto item = new(
            VulnerabilityId: first.Key.VulnerabilityId.Value,
            Title: first.Title,
            SeverityId: rows.Min(x => x.Severity.Rank),
            IsKnownExploited: first.IsKnownExploited,
            KevDateAdded: first.KevDateAdded,
            EpssPercentile: first.EpssPercentile,
            Packages:
            [
                .. rows
                    .Select(row =>
                    {
                        HomeReach rowReach = scope.GetReach(row);

                        return new HomeNewFixPackageDto(
                            PackageKey: row.Key.PackageKey.Value,
                            PackageName: row.PackageName.Value,
                            InstalledVersions: HomeImageRelations.InstalledVersionsInScope(row, scope, table),
                            FixedVersion: row.FixedVersion!.Value.Value,
                            SeverityId: row.Severity.Rank,
                            Published: row.Published?.Value,
                            FirstSeenAt: row.FirstAt?.Value,
                            FixAppearedAt: row.FixAppearedAt!.Value.Value,
                            Images: rowReach.ImageKeys.Count,
                            ImageIndexes:
                            [
                                .. rowReach.ImageKeys
                                    .Select(key => relations.ImageIndexes[ToImageName(key, table)])
                                    .Distinct()
                                    .Order(),
                            ]);
                    })
                    .OrderBy(x => x.SeverityId)
                    .ThenByDescending(x => x.Images),
            ],
            Images: reach.ImageKeys.Count,
            Namespaces: reach.Namespaces.Count,
            Workloads: reach.Workloads.Count,
            NamespaceNames: relations.NamespaceNames,
            ImageNames: relations.ImageNames,
            ImageNamespaceIndexes: relations.ImageNamespaceIndexes);

        return new HomeNewFixDetailDto(true, null, id, filter.Since, null, item);
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
                        Title: first.Title,
                        SeverityId: group.Min(x => x.Severity.Rank),
                        KevDateAdded: first.KevDateAdded,
                        KevKnownRansomwareCampaignUse: first.Enrichment?.Kev.KnownRansomwareCampaignUse,
                        EuvdExploitedSince: first.EuvdExploitedSince,
                        EpssPercentile: first.EpssPercentile,
                        IsNewSince: exploitedSince is not null && exploitedSince.Value >= filter.Since,
                        Packages: group.Count(),
                        FixablePackages: group.Count(x => x.HasFix),
                        Images: reach.ImageKeys.Count,
                        Namespaces: reach.Namespaces.Count,
                        Workloads: reach.Workloads.Count);
                })
                .OrderByDescending(x => x.IsNewSince)
                .ThenByDescending(x => x.Images)
                .ThenBy(x => x.SeverityId)
                .ThenBy(x => x.VulnerabilityId, StringComparer.Ordinal),
        ];

        return new HomeKnownExploitedDto(
            true,
            null,
            filter.Since,
            items.Count(x => x.IsNewSince),
            ToTotals(rows, scope),
            [.. items.Take(filter.Limit),]);
    }

    // one known exploited id (exact, case-insensitive), everything: its KEV / EUVD data, packages, images and namespaces
    public async Task<HomeKnownExploitedDetailDto> GetKnownExploitedDetailed(
        string vulnerabilityId,
        HomeFilter filter,
        CancellationToken ctx = default)
    {
        string id = vulnerabilityId.Trim();

        string? reason = !sources.HasVulnerabilityReports ? NoVulnerabilityReports
            : !cveEnrichmentService.IsEnabled ? NoCveEnrichment
            : null;

        if (reason is not null)
        {
            return new HomeKnownExploitedDetailDto(false, reason, id, filter.Since, null, null);
        }

        ActiveVulnerabilityTable table = await GetTable(ctx);
        HomeScope scope = new(table, filter.Namespaces);

        List<ActiveVulnerability> rows =
        [
            .. table.Rows.Where(x =>
                x.IsKnownExploited &&
                string.Equals(x.Key.VulnerabilityId.Value, id, StringComparison.OrdinalIgnoreCase) &&
                scope.Contains(x)),
        ];

        if (rows.Count == 0)
        {
            return new HomeKnownExploitedDetailDto(
                true,
                null,
                id,
                filter.Since,
                "Not known exploited and running now (in these namespaces).",
                null);
        }

        ActiveVulnerability first = rows[0];
        KevResultDto? kev = first.Enrichment?.Kev is { Status: "Ok", } ok ? ok : null;
        EuvdResultDto? euvd = first.Enrichment?.Euvd is { Status: "Ok", } okEuvd ? okEuvd : null;
        DateTime? exploitedSince = first.KnownExploitedSince;

        HomeReach reach = scope.GetReach(rows.SelectMany(x => x.ImageKeys));
        HomeImageRelations relations = HomeImageRelations.Build(reach, scope, table);

        HomeKnownExploitedDetailItemDto item = new(
            VulnerabilityId: first.Key.VulnerabilityId.Value,
            Title: first.Title,
            SeverityId: rows.Min(x => x.Severity.Rank),
            ExploitedSince: exploitedSince,
            IsNewSince: exploitedSince is not null && exploitedSince.Value >= filter.Since,
            KevDateAdded: kev?.DateAdded,
            KevDueDate: kev?.DueDate,
            KevKnownRansomwareCampaignUse: kev?.KnownRansomwareCampaignUse,
            KevRequiredAction: kev?.RequiredAction,
            EuvdId: euvd?.EuvdId,
            EuvdExploitedSince: euvd?.ExploitedSince,
            EpssPercentile: first.EpssPercentile,
            Packages:
            [
                .. rows
                    .Select(row =>
                    {
                        HomeReach rowReach = scope.GetReach(row);

                        return new HomeKnownExploitedPackageDto(
                            PackageKey: row.Key.PackageKey.Value,
                            PackageName: row.PackageName.Value,
                            InstalledVersions: HomeImageRelations.InstalledVersionsInScope(row, scope, table),
                            FixedVersion: row.HasFix ? row.FixedVersion!.Value.Value : null,
                            SeverityId: row.Severity.Rank,
                            Images: rowReach.ImageKeys.Count,
                            ImageIndexes:
                            [
                                .. rowReach.ImageKeys
                                    .Select(key => relations.ImageIndexes[ToImageName(key, table)])
                                    .Distinct()
                                    .Order(),
                            ]);
                    })
                    .OrderBy(x => x.SeverityId)
                    .ThenByDescending(x => x.Images),
            ],
            Images: reach.ImageKeys.Count,
            Namespaces: reach.Namespaces.Count,
            Workloads: reach.Workloads.Count,
            NamespaceNames: relations.NamespaceNames,
            ImageNames: relations.ImageNames,
            ImageNamespaceIndexes: relations.ImageNamespaceIndexes);

        return new HomeKnownExploitedDetailDto(true, null, id, filter.Since, null, item);
    }

    // the rows of the new vulnerabilities, flat: one per (vulnerability id, package, image)
    public async Task<HomeVulnerabilityRowsDto> GetNewVulnerabilitiesDenormalized(
        HomeFilter filter,
        CancellationToken ctx = default)
    {
        string? reason = !sources.HasVulnerabilityReports ? NoVulnerabilityReports
            : !vulnerabilityReportsHistoryService.IsEnabled ? NoHistory
            : null;

        if (reason is not null)
        {
            return new HomeVulnerabilityRowsDto(false, reason, filter.Since, []);
        }

        ActiveVulnerabilityTable table = await GetTable(ctx);
        HomeScope scope = new(table, filter.Namespaces);
        HashSet<int> severities = ToSeverities(filter.SeverityIds);

        return new HomeVulnerabilityRowsDto(
            true,
            null,
            filter.Since,
            ToVulnerabilityRows(table.Rows.Where(x => IsNew(x, filter.Since, severities, scope)), scope, table));
    }

    // the rows of the new fixes, flat
    public async Task<HomeVulnerabilityRowsDto> GetNewFixesDenormalized(
        HomeFilter filter,
        CancellationToken ctx = default)
    {
        string? reason = !sources.HasVulnerabilityReports ? NoVulnerabilityReports
            : !vulnerabilityReportsHistoryService.IsEnabled ? NoHistory
            : null;

        if (reason is not null)
        {
            return new HomeVulnerabilityRowsDto(false, reason, filter.Since, []);
        }

        ActiveVulnerabilityTable table = await GetTable(ctx);
        HomeScope scope = new(table, filter.Namespaces);
        HashSet<int> severities = ToSeverities(filter.SeverityIds);

        return new HomeVulnerabilityRowsDto(
            true,
            null,
            filter.Since,
            ToVulnerabilityRows(table.Rows.Where(x => IsNewFix(x, filter.Since, severities, scope)), scope, table));
    }

    // the rows of the known exploited ones, flat (no severity filter, as the list)
    public async Task<HomeVulnerabilityRowsDto> GetKnownExploitedDenormalized(
        HomeFilter filter,
        CancellationToken ctx = default)
    {
        string? reason = !sources.HasVulnerabilityReports ? NoVulnerabilityReports
            : !cveEnrichmentService.IsEnabled ? NoCveEnrichment
            : null;

        if (reason is not null)
        {
            return new HomeVulnerabilityRowsDto(false, reason, filter.Since, []);
        }

        ActiveVulnerabilityTable table = await GetTable(ctx);
        HomeScope scope = new(table, filter.Namespaces);

        return new HomeVulnerabilityRowsDto(
            true,
            null,
            filter.Since,
            ToVulnerabilityRows(table.Rows.Where(x => x.IsKnownExploited && scope.Contains(x)), scope, table));
    }

    // one row per (vulnerability id, package) row and image in scope, with the versions of the package in that image and
    // the namespaces the image runs in
    private static IReadOnlyList<HomeVulnerabilityRowDto> ToVulnerabilityRows(
        IEnumerable<ActiveVulnerability> rows,
        HomeScope scope,
        ActiveVulnerabilityTable table)
    {
        List<HomeVulnerabilityRowDto> result = [];

        foreach (ActiveVulnerability row in rows)
        {
            foreach (string imageKey in scope.GetReach(row).ImageKeys)
            {
                if (!table.Images.TryGetValue(imageKey, out ActiveImage? image))
                {
                    continue;
                }

                result.Add(new HomeVulnerabilityRowDto(
                    VulnerabilityId: row.Key.VulnerabilityId.Value,
                    Title: row.Title,
                    SeverityId: row.Severity.Rank,
                    IsKnownExploited: row.IsKnownExploited,
                    EpssPercentile: row.EpssPercentile,
                    PackageName: row.PackageName.Value,
                    InstalledVersions:
                    [
                        .. row.ImageKeysByInstalledVersion
                            .Where(x => x.Value.Any(key => string.Equals(key, imageKey, StringComparison.Ordinal)))
                            .Select(x => x.Key)
                            .Order(StringComparer.Ordinal),
                    ],
                    FixedVersion: row.HasFix ? row.FixedVersion!.Value.Value : null,
                    FirstSeenAt: row.FirstAt?.Value,
                    FixSince: row.FixSince?.Value,
                    FixAppearedAt: row.FixAppearedAt?.Value,
                    ExploitedSince: row.KnownExploitedSince,
                    KevDateAdded: row.KevDateAdded,
                    ImageRegistry: image.ImageMeta.Registry.Value,
                    ImageName: $"{image.ImageMeta.Repo.Value}:{image.ImageMeta.Tag.Value}",
                    ImageDigest: image.Digest?.Value ?? string.Empty,
                    NamespaceNames: ToNamespaceNames(scope.GetReach(new[] { imageKey, }).Namespaces, int.MaxValue)));
            }
        }

        return
        [
            .. result
                .OrderBy(x => x.SeverityId)
                .ThenBy(x => x.VulnerabilityId, StringComparer.Ordinal)
                .ThenBy(x => x.PackageName, StringComparer.Ordinal)
                .ThenBy(x => x.ImageName, StringComparer.Ordinal),
        ];
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
                // the versions Trivy reports (alpine 3.19.0, 3.19.1) are one OS cycle (3.19) when the end of life data has it;
                // the ones without are left as they are
                .Select(group => (group.Key.Family, group.Key.Version, Info: eolInfos.GetValueOrDefault(group.Key), Images: group))
                .GroupBy(x => (x.Family, Cycle: x.Info?.Release?.Cycle ?? x.Version))
                .Select(group =>
                {
                    EolInfoDto? info = group.First().Info;
                    EolReleaseDto? release = info?.Release;
                    // the cycle counts each image, namespace and workload once
                    HomeReach reach = scope.GetReach(group.SelectMany(x => x.Images).Select(x => x.Key));

                    return new HomeBaseOsItemDto(
                        Family: group.Key.Family,
                        Version: group.Key.Cycle,
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
                        Workloads: reach.Workloads.Count,
                        ImageDetails: HomeNames.ToBaseOsImages(reach.ImageKeys, table, filter.Limit),
                        NamespaceNames: [.. reach.Namespaces.Select(x => x.Value).Order(StringComparer.Ordinal).Take(filter.Limit),]);
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

    public async Task<HomeBaseOsImagesDto> GetBaseOsImages(CancellationToken ctx = default)
    {
        string? reason = !sources.HasVulnerabilityReports ? NoVulnerabilityReports
            : !endOfLifeService.IsEnabled ? NoEndOfLife
            : null;

        if (reason is not null)
        {
            return new HomeBaseOsImagesDto(false, reason, []);
        }

        ActiveVulnerabilityTable table = await GetTable(ctx);

        // same normalization and cycle as GetBaseOs
        Dictionary<(string Family, string Version), EolInfoDto> eolInfos = (await endOfLifeService.GetAll(EolSubjectKind.Os, ctx))
            .GroupBy(x => (x.Name, x.Version))
            .ToDictionary(x => x.Key, x => x.First());

        List<HomeBaseOsImageRowDto> items =
        [
            .. table.Images.Values
                .Where(x => x.Os.Family.IsValid && x.Os.Name.IsValid)
                .SelectMany(image =>
                {
                    (string Family, string Version) key =
                        (image.Os.Family.Value.Trim().ToLowerInvariant(), image.Os.Name.Value.Trim());
                    EolReleaseDto? release = eolInfos.GetValueOrDefault(key)?.Release;

                    // a cluster scoped image has no namespace
                    IEnumerable<string?> namespaces = image.WorkloadsByNamespace.Count == 0
                        ? [null,]
                        : image.WorkloadsByNamespace.Keys.Select(x => (string?)x.Value);

                    return namespaces.Select(namespaceName => new HomeBaseOsImageRowDto(
                        OsName: key.Family,
                        OsVersion: image.Os.Name.Value,
                        OsEolFrom: release?.EolFrom,
                        OsEoasFrom: release?.EoasFrom,
                        NamespaceName: namespaceName,
                        ImageRegistry: image.ImageMeta.Registry.Value,
                        ImageName: image.ImageMeta.Repo.Value,
                        ImageTag: image.ImageMeta.Tag.Value,
                        ImageDigest: image.Digest?.Value ?? string.Empty));
                }),
        ];

        return new HomeBaseOsImagesDto(true, null, items);
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

    // one id, everything: the rows of the id with a fix (the same rule as the list), their images and namespaces
    public async Task<HomeFixWaitingDetailDto> GetFixWaitingDetailed(
        string vulnerabilityId,
        HomeFilter filter,
        CancellationToken ctx = default)
    {
        string id = vulnerabilityId.Trim();

        if (!sources.HasVulnerabilityReports)
        {
            return new HomeFixWaitingDetailDto(false, NoVulnerabilityReports, id, null, null);
        }

        ActiveVulnerabilityTable table = await GetTable(ctx);
        HomeScope scope = new(table, filter.Namespaces);
        HashSet<int> severities = ToSeverities(filter.SeverityIds);

        List<ActiveVulnerability> rows =
        [
            .. table.Rows.Where(x =>
                string.Equals(x.Key.VulnerabilityId.Value, id, StringComparison.OrdinalIgnoreCase) &&
                severities.Contains(x.Severity.Rank) && x.HasFix && scope.Contains(x)),
        ];

        if (rows.Count == 0)
        {
            return new HomeFixWaitingDetailDto(
                true,
                null,
                id,
                "No fix waiting in these severities and namespaces.",
                null);
        }

        ActiveVulnerability first = rows[0];
        HomeReach reach = scope.GetReach(rows.SelectMany(x => x.ImageKeys));
        HomeImageRelations relations = HomeImageRelations.Build(reach, scope, table);

        HomeFixWaitingDetailItemDto item = new(
            VulnerabilityId: first.Key.VulnerabilityId.Value,
            Title: first.Title,
            SeverityId: rows.Min(x => x.Severity.Rank),
            IsKnownExploited: first.IsKnownExploited,
            KevDateAdded: first.KevDateAdded,
            EpssPercentile: first.EpssPercentile,
            Packages:
            [
                .. rows
                    .Select(row =>
                    {
                        HomeReach rowReach = scope.GetReach(row);

                        return new HomeFixWaitingPackageDto(
                            PackageKey: row.Key.PackageKey.Value,
                            PackageName: row.PackageName.Value,
                            InstalledVersions: HomeImageRelations.InstalledVersionsInScope(row, scope, table),
                            FixedVersion: row.FixedVersion!.Value.Value,
                            SeverityId: row.Severity.Rank,
                            Published: row.Published?.Value,
                            FirstSeenAt: row.FirstAt?.Value,
                            FixSince: row.FixSince?.Value,
                            FixAppearedAt: row.FixAppearedAt?.Value,
                            Images: rowReach.ImageKeys.Count,
                            ImageIndexes:
                            [
                                .. rowReach.ImageKeys
                                    .Select(key => relations.ImageIndexes[ToImageName(key, table)])
                                    .Distinct()
                                    .Order(),
                            ]);
                    })
                    // the longest waiting first; without history by reach only
                    .OrderBy(x => x.FixSince is null)
                    .ThenBy(x => x.FixSince)
                    .ThenByDescending(x => x.Images),
            ],
            Images: reach.ImageKeys.Count,
            Namespaces: reach.Namespaces.Count,
            Workloads: reach.Workloads.Count,
            NamespaceNames: relations.NamespaceNames,
            ImageNames: relations.ImageNames,
            ImageNamespaceIndexes: relations.ImageNamespaceIndexes);

        return new HomeFixWaitingDetailDto(true, null, id, null, item);
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

        List<HomeAssessmentReport> reports =
        [
            .. (await assessmentReportsLoader.Load(ctx))
                .Where(report => scope.Contains(report.NamespaceName, report.IsClusterScoped)),
        ];

        // the resources evaluated by a check are estimated by kind: the reports in the scope by family and kind bin
        Dictionary<(string Family, string Bin), int> evaluated = reports
            .GroupBy(report => (report.Family, KindBin(report)))
            .ToDictionary(group => group.Key, group => group.Select(x => x.Uid).Distinct().Count());

        List<HomeFailingCheckDto> items =
        [
            .. reports
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
                        Category: check.Category.Value,
                        SeverityId: check.Severity.Rank,
                        Resources: group.Select(x => x.Report.Uid).Distinct().Count(),
                        Evaluated: group
                            .Select(x => KindBin(x.Report))
                            .Distinct()
                            .Sum(bin => evaluated[(group.Key.Family, bin)]),
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

    // one check (family and id, exact), the reports in the scope that fail it, one row per failing resource
    public async Task<HomeFailingCheckDetailDto> GetFailingCheckDetailed(
        string family,
        string checkId,
        HomeFilter filter,
        CancellationToken ctx = default)
    {
        string familyValue = family.Trim();
        string id = checkId.Trim();

        if (!sources.HasSecurityAssessmentReports)
        {
            return new HomeFailingCheckDetailDto(false, NoSecurityAssessments, familyValue, id, null, null);
        }

        HomeScope scope = new(EmptyTable, filter.Namespaces);

        List<HomeAssessmentReport> familyReports =
        [
            .. (await assessmentReportsLoader.Load(ctx))
                .Where(report => string.Equals(report.Family, familyValue, StringComparison.Ordinal) &&
                                 scope.Contains(report.NamespaceName, report.IsClusterScoped)),
        ];

        List<(HomeAssessmentReport Report, Check Check)> group =
        [
            .. familyReports
                .SelectMany(report => report.Checks.Select(check => (Report: report, Check: check)))
                .Where(x => string.Equals(x.Check.CheckId.Value, id, StringComparison.Ordinal)),
        ];

        if (group.Count == 0)
        {
            return new HomeFailingCheckDetailDto(
                true,
                null,
                familyValue,
                id,
                "This check does not fail now in these namespaces.",
                null);
        }

        Check first = group[0].Check;
        HomeAssessmentReport[] reports = [.. group.Select(x => x.Report).DistinctBy(x => x.Uid),];

        // the evaluated resources: the reports of the family in the kind bins of the failing ones
        HashSet<string> bins = [.. reports.Select(KindBin),];
        HomeAssessmentReport[] evaluatedReports =
            [.. familyReports.Where(x => bins.Contains(KindBin(x))).DistinctBy(x => x.Uid),];
        Dictionary<string, int> evaluatedByNamespace = evaluatedReports
            .GroupBy(NamespaceKey)
            .ToDictionary(x => x.Key, x => x.Count());

        HomeFailingCheckDetailItemDto item = new(
            Family: familyValue,
            CheckId: id,
            Title: first.Title.Value,
            SeverityId: first.Severity.Rank,
            Category: first.Category.Value,
            Description: first.Description.Value,
            Remediation: first.Remediation.Value,
            Resources: reports.Length,
            Evaluated: evaluatedReports.Length,
            Namespaces: reports.Where(x => !x.IsClusterScoped).Select(x => x.NamespaceName).Distinct().Count(),
            HasClusterScopedResources: reports.Any(x => x.IsClusterScoped),
            ByNamespace:
            [
                .. reports
                    .GroupBy(NamespaceKey)
                    .Select(x => new HomeFailingCheckNamespaceDto(
                        NamespaceName: x.Key.Length == 0 ? null : x.Key,
                        Resources: x.Count(),
                        Evaluated: evaluatedByNamespace.GetValueOrDefault(x.Key)))
                    .OrderBy(x => x.NamespaceName is null)
                    .ThenByDescending(x => x.Resources)
                    .ThenBy(x => x.NamespaceName, StringComparer.Ordinal),
            ],
            Rows:
            [
                .. reports
                    .Select(ToCheckResource)
                    .OrderBy(x => x.NamespaceName is null)
                    .ThenBy(x => x.NamespaceName, StringComparer.Ordinal)
                    .ThenBy(x => x.ResourceKind, StringComparer.Ordinal)
                    .ThenBy(x => x.ResourceName, StringComparer.Ordinal),
            ]);

        return new HomeFailingCheckDetailDto(true, null, familyValue, id, null, item);
    }

    // the assessed resource: the owner of the report when there is one, else the report
    private static HomeWhereIsCheckResourceDto ToCheckResource(HomeAssessmentReport report)
    {
        OwnerReference? owner = report.Metadata.OwnerReferences.Count > 0
            ? report.Metadata.OwnerReferences[0]
            : (OwnerReference?)null;

        return new HomeWhereIsCheckResourceDto(
            ReportKind: report.Kind,
            NamespaceName: report.IsClusterScoped ? null : report.NamespaceName.Value,
            ResourceKind: owner?.Kind.Value ?? report.Kind,
            ResourceName: owner?.Name.Value ?? report.Metadata.Name.Value);
    }

    // the kinds that run pods: trivy reports the one that owns them (a ReplicaSet for a Deployment, a Job for a CronJob,
    // a Pod alone), so a check that fails in one of them is evaluated in all of them
    private static readonly HashSet<string> WorkloadKinds = new(StringComparer.Ordinal)
    {
        "Pod", "ReplicaSet", "ReplicationController", "Deployment", "StatefulSet", "DaemonSet", "Job", "CronJob",
    };

    // the bin of the kind of the assessed resource (the owner of the report, else the report): the resources a check
    // evaluates are those of the bins it fails in
    private static string KindBin(HomeAssessmentReport report)
    {
        string kind = report.Metadata.OwnerReferences.Count > 0
            ? report.Metadata.OwnerReferences[0].Kind.Value
            : report.Kind;

        return WorkloadKinds.Contains(kind) ? "Workload"
            : kind is "Role" or "ClusterRole" ? "Role"
            : kind is "RoleBinding" or "ClusterRoleBinding" ? "RoleBinding"
            : kind;
    }

    // the namespace of a report, "" for a cluster scoped one
    private static string NamespaceKey(HomeAssessmentReport report) =>
        report.IsClusterScoped ? string.Empty : report.NamespaceName.Value;

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
                    List<ActiveVulnerability> imageRows =
                    [
                        .. image.Keys
                            .Select(key => rows.GetValueOrDefault(key))
                            .OfType<ActiveVulnerability>()
                            .Where(row => severities.Contains(row.Severity.Rank)),
                    ];

                    return new HomeOldestImageDto(
                        Digest: image.Digest!.Value.Value,
                        Registry: image.ImageMeta.Registry.Value,
                        Repository: image.ImageMeta.Repo.Value,
                        Tag: image.ImageMeta.Tag.Value,
                        FirstSeenAt: image.FirstSeenAt!.Value.Value,
                        Workloads: reach.Workloads.Count,
                        Namespaces: reach.Namespaces.Count,
                        NamespaceNames: ToNamespaceNames(reach.Namespaces),
                        OsFamily: image.Os.Family.IsValid ? image.Os.Family.Value : null,
                        OsName: image.Os.Name.IsValid ? image.Os.Name.Value : null,
                        IsOsEndOfLife: image.Os.IsEndOfLife,
                        VulnerabilityIds: imageRows.Select(row => row.Key.VulnerabilityId).Distinct().Count(),
                        FixableVulnerabilityIds: imageRows
                            .Where(row => row.HasFix)
                            .Select(row => row.Key.VulnerabilityId)
                            .Distinct()
                            .Count(),
                        KnownExploitedVulnerabilityIds: imageRows
                            .Where(row => row.IsKnownExploited)
                            .Select(row => row.Key.VulnerabilityId)
                            .Distinct()
                            .Count(),
                        HighestSeverityId: imageRows.Count > 0 ? imageRows.Min(row => row.Severity.Rank) : null);
                }),
        ];

        return new HomeOldestImagesDto(true, null, table.HistoryStartedAt?.Value, items);
    }

    // running images with exposed secrets (reports of the namespaces asked); the secret values are never read into the dto
    public async Task<HomeExposedSecretsDto> GetExposedSecrets(HomeFilter filter, CancellationToken ctx = default)
    {
        if (sources.ExposedSecretReports is null)
        {
            return new HomeExposedSecretsDto(false, NoExposedSecrets, 0, null, 0, 0, []);
        }

        HashSet<NamespaceName>? namespaceNames = filter.Namespaces is { Count: > 0, }
            ? [.. filter.Namespaces.Select(x => new NamespaceName(x)),]
            : null;

        // the reach of a report counts only the occurrences inside the namespaces asked
        List<(ExposedSecretReport Report, List<ReportImageOccurrence> Occurrences)> scoped =
        [
            .. (await sources.ExposedSecretReports.GetResources(ctx))
                .Select(report => (
                    Report: report,
                    Occurrences: report.Occurrences
                        .Where(x => namespaceNames is null || namespaceNames.Contains(x.Metadata.NamespaceName))
                        .ToList()))
                .Where(x => x.Occurrences.Count > 0),
        ];

        List<HomeExposedSecretImageDto> images =
        [
            .. scoped
                .Where(x => x.Report.Secrets.Count > 0)
                .Select(x =>
                {
                    ReportImageOccurrence first = x.Occurrences[0];
                    HashSet<NamespaceName> reachedNamespaces = [.. x.Occurrences.Select(o => o.Metadata.NamespaceName),];

                    return new HomeExposedSecretImageDto(
                        Digest: x.Report.ImageDigest.Value,
                        Registry: first.ImageMeta.Registry.Value,
                        Repository: first.ImageMeta.Repo.Value,
                        Tag: first.ImageMeta.Tag.Value,
                        SecretCount: x.Report.Secrets.Count,
                        SeverityId: x.Report.Secrets.Min(s => s.Rule.Severity.Rank),
                        Workloads: x.Occurrences.Select(o => o.Metadata.Uid).Distinct().Count(),
                        Namespaces: reachedNamespaces.Count,
                        NamespaceNames: ToNamespaceNames(reachedNamespaces),
                        ByType:
                        [
                            .. x.Report.Secrets
                                .GroupBy(s => (Type: s.Rule.Title.Value, SeverityId: s.Rule.Severity.Rank))
                                .Select(g => new HomeExposedSecretTypeCountDto(g.Key.Type, g.Key.SeverityId, g.Count()))
                                .OrderBy(g => g.SeverityId)
                                .ThenByDescending(g => g.Count)
                                .ThenBy(g => g.Type, StringComparer.Ordinal),
                        ]);
                })
                .OrderBy(x => x.SeverityId)
                .ThenByDescending(x => x.SecretCount)
                .ThenBy(x => x.Repository, StringComparer.Ordinal)
                .ThenBy(x => x.Digest, StringComparer.Ordinal),
        ];

        return new HomeExposedSecretsDto(
            true,
            null,
            scoped.Count,
            scoped.Count == 0 ? null : scoped.Max(x => x.Report.LastSeenAt).Value,
            images.Sum(x => x.SecretCount),
            images.Count,
            [.. images.Take(filter.Limit),]);
    }

    // ------------------------------------------------------------------------
    // Compliance
    // ------------------------------------------------------------------------

    public async Task<HomeComplianceDto> GetCompliance(CancellationToken ctx = default)
    {
        if (sources.ClusterComplianceReports is null)
        {
            return new HomeComplianceDto(false, NoCompliance, []);
        }

        return new HomeComplianceDto(
            true,
            null,
            [
                .. (await sources.ClusterComplianceReports.GetResources(ctx))
                    .Select(report =>
                    {
                        int total = report.Summary.PassCount + report.Summary.FailCount;

                        return new HomeComplianceReportDto(
                            Uid: report.Metadata.Uid.Value,
                            Name: report.Metadata.Name.Value,
                            Title: report.ComplianceMetadata.Title.Value,
                            Version: report.ComplianceMetadata.Version.Value,
                            PassCount: report.Summary.PassCount,
                            FailCount: report.Summary.FailCount,
                            PassPercent: total == 0 ? null : report.Summary.PassCount * 100.0 / total,
                            LastScanAt: report.LastSeenAt.Value);
                    })
                    .OrderBy(x => x.Name, StringComparer.Ordinal)
                    .ThenBy(x => x.Uid, StringComparer.Ordinal),
            ]);
    }

    public async Task<HomeComplianceWorstControlsDto> GetComplianceWorstControls(
        string reportUid,
        HomeFilter filter,
        CancellationToken ctx = default)
    {
        if (sources.ClusterComplianceReports is null)
        {
            return new HomeComplianceWorstControlsDto(false, NoCompliance, reportUid, string.Empty, string.Empty, 0, []);
        }

        ClusterComplianceReport? report = (await sources.ClusterComplianceReports.GetResources(ctx))
            .FirstOrDefault(x => string.Equals(x.Metadata.Uid.Value, reportUid, StringComparison.Ordinal));

        if (report is null)
        {
            return new HomeComplianceWorstControlsDto(false, "Compliance report not found", reportUid, string.Empty, string.Empty, 0, []);
        }

        List<HomeComplianceControlDto> failing =
        [
            .. report.ControlChecks
                .Where(x => x.TotalFail.Value > 0)
                .Select(x => new HomeComplianceControlDto(
                    ControlId: x.Control.Id.Value,
                    Name: x.Control.ControlName.Value,
                    SeverityId: x.Control.Severity.Rank,
                    TotalFail: x.TotalFail.Value))
                .OrderByDescending(x => x.TotalFail)
                .ThenBy(x => x.SeverityId)
                .ThenBy(x => x.ControlId, StringComparer.Ordinal),
        ];

        return new HomeComplianceWorstControlsDto(
            true,
            null,
            reportUid,
            report.ComplianceMetadata.Title.Value,
            report.ComplianceMetadata.Version.Value,
            failing.Count,
            [.. failing.Take(filter.Limit),]);
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

    public async Task<HomeReachEpssDto> GetReachEpss(HomeFilter filter, double? minEpss = null, CancellationToken ctx = default)
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
                    .Where(x => severities.Contains(x.Severity.Rank) && x.EpssPercentile is not null && x.EpssPercentile >= (minEpss ?? 0) && scope.Contains(x))
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

    // new: first seen since the window start (never for a row that existed before the history started recording)
    private static bool IsNew(ActiveVulnerability row, DateTime since, HashSet<int> severities, HomeScope scope) =>
        severities.Contains(row.Severity.Rank) &&
        row.FirstAt is { } firstAt && firstAt.Value >= since &&
        scope.Contains(row);

    // new fix: the fix appeared since the window start (not for a row that came with its fix)
    private static bool IsNewFix(ActiveVulnerability row, DateTime since, HashSet<int> severities, HomeScope scope) =>
        severities.Contains(row.Severity.Rank) && row.HasFix &&
        row.FixAppearedAt is { } fixAppearedAt && fixAppearedAt.Value >= since &&
        scope.Contains(row);

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
            Title: row.Title,
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
