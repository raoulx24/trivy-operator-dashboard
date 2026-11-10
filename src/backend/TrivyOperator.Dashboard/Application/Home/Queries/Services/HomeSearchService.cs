using System.Text.RegularExpressions;
using TrivyOperator.Dashboard.Application.Home.Models;
using TrivyOperator.Dashboard.Application.Home.Queries.Models;
using TrivyOperator.Dashboard.Application.Home.Queries.Services.Abstractions;
using TrivyOperator.Dashboard.Application.Home.Services;
using TrivyOperator.Dashboard.Application.Shared.Queries.Services;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Sboms;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.SecurityAssessments;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Vulnerabilities;
using static TrivyOperator.Dashboard.Application.Home.Services.HomeNames;

namespace TrivyOperator.Dashboard.Application.Home.Queries.Services;

public partial class HomeSearchService(
    HomeReportSources sources,
    ActiveVulnerabilityTableCache tableCache,
    ActiveVulnerabilityTableBuilder tableBuilder,
    HomeAssessmentReportsLoader assessmentReportsLoader
) : QueryService, IHomeSearchService
{
    private const string NoVulnerabilityReports = "Vulnerability reports are not enabled";
    private const string NoSbom = "SBOM reports are not enabled";
    private const string NoSecurityAssessments = "No security assessment reports (config audit, RBAC, infra) are enabled";

    // "CVE-" alone would match almost everything
    private const int MinVulnerabilityQueryLength = 4;
    private const int MinQueryLength = 3;
    private const int MinLicenseQueryLength = 2;

    private const int MaxPackagesPerLicense = 20;
    private const int MaxSampleResources = 20;

    // ------------------------------------------------------------------------
    // vulnerability
    // ------------------------------------------------------------------------

    public async Task<HomeWhereIsVulnerabilityDto> FindVulnerability(
        string query,
        HomeFilter filter,
        CancellationToken ctx = default)
    {
        string q = query.Trim();

        if (!sources.HasVulnerabilityReports)
        {
            return new HomeWhereIsVulnerabilityDto(false, NoVulnerabilityReports, q, null, 0, []);
        }

        if (q.Length < MinVulnerabilityQueryLength)
        {
            return new HomeWhereIsVulnerabilityDto(true, null, q, TooShort(MinVulnerabilityQueryLength), 0, []);
        }

        ActiveVulnerabilityTable table = await GetTable(ctx);
        HomeScope scope = new(table, filter.Namespaces);

        List<HomeWhereIsVulnerabilityItemDto> items =
        [
            .. table.Rows
                .Where(x => x.Key.VulnerabilityId.Value.StartsWith(q, StringComparison.OrdinalIgnoreCase) && scope.Contains(x))
                .GroupBy(x => x.Key.VulnerabilityId)
                .Select(group =>
                {
                    ActiveVulnerability first = group.First();
                    HomeReach reach = scope.GetReach(group.SelectMany(x => x.ImageKeys));

                    return new HomeWhereIsVulnerabilityItemDto(
                        VulnerabilityId: group.Key.Value,
                        Title: first.Title,
                        SeverityId: group.Min(x => x.Severity.Rank),
                        IsKnownExploited: first.IsKnownExploited,
                        KevDateAdded: first.KevDateAdded,
                        EpssPercentile: first.EpssPercentile,
                        Packages:
                        [
                            .. group
                                .Select(row => new HomeWhereIsVulnerabilityPackageDto(
                                    PackageKey: row.Key.PackageKey.Value,
                                    PackageName: row.PackageName.Value,
                                    InstalledVersions: InstalledVersionsInScope(row, scope, table),
                                    FixedVersion: row.HasFix ? row.FixedVersion!.Value.Value : null,
                                    SeverityId: row.Severity.Rank,
                                    Images: scope.GetReach(row).ImageKeys.Count))
                                .OrderBy(x => x.SeverityId)
                                .ThenByDescending(x => x.Images),
                        ],
                        Images: reach.ImageKeys.Count,
                        Namespaces: reach.Namespaces.Count,
                        Workloads: reach.Workloads.Count,
                        NamespaceNames: ToNamespaceNames(reach.Namespaces),
                        ImageNames: ToImageNames(reach.ImageKeys, table));
                })
                .OrderByDescending(x => string.Equals(x.VulnerabilityId, q, StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(x => x.Images)
                .ThenBy(x => x.SeverityId)
                .ThenBy(x => x.VulnerabilityId, StringComparer.Ordinal),
        ];

        // the reports do not carry the aliases, so a CVE reported under another id is not found by its CVE
        string? note = items.Count == 0
            ? "Not found. Trivy may report it under another id (e.g. a GHSA or a distro advisory); aliases are not known here."
            : null;

        return new HomeWhereIsVulnerabilityDto(true, null, q, note, items.Count, [.. items.Take(filter.Limit),]);
    }

    private static IReadOnlyList<string> InstalledVersionsInScope(
        ActiveVulnerability row,
        HomeScope scope,
        ActiveVulnerabilityTable table) =>
    [
        .. row.ImageKeysByInstalledVersion
            .Where(x => x.Value.Any(key => scope.Contains(table.Images[key])))
            .Select(x => x.Key)
            .Order(StringComparer.Ordinal),
    ];

    // ------------------------------------------------------------------------
    // package
    // ------------------------------------------------------------------------

    public async Task<HomeWhereIsPackageDto> FindPackage(string query, HomeFilter filter, CancellationToken ctx = default)
    {
        string q = query.Trim();
        string source = sources.HasSbom ? "Sbom" : "VulnerabilityReports";

        // the images and namespaces come from the vulnerability reports, also when the packages come from the sbom
        if (!sources.HasVulnerabilityReports)
        {
            return new HomeWhereIsPackageDto(false, NoVulnerabilityReports, q, null, source, 0, []);
        }

        if (q.Length < MinQueryLength)
        {
            return new HomeWhereIsPackageDto(true, null, q, TooShort(MinQueryLength), source, 0, []);
        }

        ActiveVulnerabilityTable table = await GetTable(ctx);
        HomeScope scope = new(table, filter.Namespaces);
        PackageMatcher matcher = PackageMatcher.Parse(q);

        // (package key, version) -> images, plus the purls for the licenses
        Dictionary<string, PackageMatch> matches = new(StringComparer.Ordinal);
        string? note;

        if (sources.HasSbom)
        {
            (IReadOnlyDictionary<Digest, ImagePurls> imagePurls, int imagesWithoutSbom) = await GetImagePurls(table, scope, ctx);

            foreach ((Digest digest, ImagePurls purls) in imagePurls)
            {
                foreach (Purl purl in purls.Purls)
                {
                    Purl.Components components = purl.Parse();
                    string packageKey = PackageKey.FromPurl(purl)?.Value ?? purl.Value;
                    string name = components.Name ?? purl.Value;
                    string version = components.Version ?? string.Empty;

                    if (matcher.Matches(packageKey, name, version))
                    {
                        GetMatch(matches, packageKey, name, components.Type).Add(version, digest.Value, purl);
                    }
                }
            }

            IReadOnlyDictionary<Purl, PurlDetail> purlDetails =
                await sources.PurlDetails!.GetDetails(matches.Values.SelectMany(x => x.Purls).Distinct(), ctx);

            foreach (PackageMatch match in matches.Values)
            {
                match.Licenses.UnionWith(match.Purls
                    .Select(x => purlDetails.GetValueOrDefault(x))
                    .OfType<PurlDetail>()
                    .SelectMany(x => x.Licenses)
                    .Select(ToLicenseLabel));
            }

            note = WithoutSbomNote(imagesWithoutSbom);
        }
        else
        {
            foreach (ActiveVulnerability row in table.Rows)
            {
                foreach ((string version, HashSet<string> imageKeys) in row.ImageKeysByInstalledVersion)
                {
                    if (!matcher.Matches(row.Key.PackageKey.Value, row.PackageName.Value, version))
                    {
                        continue;
                    }

                    PackageMatch match = GetMatch(matches, row.Key.PackageKey.Value, row.PackageName.Value, new Purl(row.Key.PackageKey.Value).Type);

                    foreach (string imageKey in imageKeys.Where(key => scope.Contains(table.Images[key])))
                    {
                        match.Add(version, imageKey, null);
                    }
                }
            }

            note = "Only the vulnerable packages are searched (SBOM reports are not enabled).";
        }

        // the active vulnerabilities of a version of a package: the rows of the package found in that version
        ILookup<string, ActiveVulnerability> rowsByPackageKey = table.Rows.ToLookup(x => x.Key.PackageKey.Value, StringComparer.Ordinal);

        List<HomeWhereIsPackageItemDto> items =
        [
            .. matches.Values
                .Where(x => x.ImageKeysByVersion.Count > 0)
                .Select(match =>
                {
                    HomeReach reach = scope.GetReach(match.ImageKeysByVersion.Values.SelectMany(x => x));

                    return new HomeWhereIsPackageItemDto(
                        PackageKey: match.PackageKey,
                        Name: match.Name,
                        Type: match.Type,
                        Licenses: [.. match.Licenses.Order(StringComparer.Ordinal),],
                        Versions:
                        [
                            .. match.ImageKeysByVersion
                                .Select(version =>
                                {
                                    HomeReach versionReach = scope.GetReach(version.Value);
                                    ActiveVulnerability[] vulnerable =
                                    [
                                        .. rowsByPackageKey[match.PackageKey]
                                            .Where(row => row.ImageKeysByInstalledVersion.ContainsKey(version.Key)),
                                    ];

                                    return new HomeWhereIsPackageVersionDto(
                                        Version: version.Key,
                                        Images: versionReach.ImageKeys.Count,
                                        Namespaces: versionReach.Namespaces.Count,
                                        Workloads: versionReach.Workloads.Count,
                                        VulnerabilityIds: vulnerable.Select(x => x.Key.VulnerabilityId).Distinct().Count(),
                                        MaxSeverityId: vulnerable.Length == 0 ? null : vulnerable.Min(x => x.Severity.Rank),
                                        ImageNames: ToImageNames(versionReach.ImageKeys, table));
                                })
                                .OrderByDescending(x => x.Images)
                                .ThenBy(x => x.Version, StringComparer.Ordinal),
                        ],
                        Images: reach.ImageKeys.Count,
                        Namespaces: reach.Namespaces.Count);
                })
                .OrderByDescending(x => string.Equals(x.Name, q, StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(x => x.Images)
                .ThenBy(x => x.PackageKey, StringComparer.Ordinal),
        ];

        return new HomeWhereIsPackageDto(true, null, q, note, source, items.Count, [.. items.Take(filter.Limit),]);
    }

    private static PackageMatch GetMatch(Dictionary<string, PackageMatch> matches, string packageKey, string name, string? type)
    {
        if (!matches.TryGetValue(packageKey, out PackageMatch? match))
        {
            match = new PackageMatch(packageKey, name, type);
            matches[packageKey] = match;
        }

        return match;
    }

    // "pkg:..." = purl prefix, matched on the package key (purl without version) and, after an "@", on the version;
    // anything else = name substring; all case-insensitive
    private sealed record PackageMatcher(string? KeyPrefix, string? VersionPrefix, string? NameSubstring)
    {
        public static PackageMatcher Parse(string query)
        {
            if (!query.StartsWith("pkg:", StringComparison.OrdinalIgnoreCase))
            {
                return new PackageMatcher(null, null, query);
            }

            int at = query.IndexOf('@');

            return at < 0
                ? new PackageMatcher(query, null, null)
                : new PackageMatcher(query[..at], query[(at + 1)..], null);
        }

        public bool Matches(string packageKey, string name, string version) =>
            NameSubstring is not null
                ? name.Contains(NameSubstring, StringComparison.OrdinalIgnoreCase)
                : packageKey.StartsWith(KeyPrefix!, StringComparison.OrdinalIgnoreCase) &&
                  (string.IsNullOrEmpty(VersionPrefix) || version.StartsWith(VersionPrefix, StringComparison.OrdinalIgnoreCase));
    }

    private sealed class PackageMatch(string packageKey, string name, string? type)
    {
        public string PackageKey { get; } = packageKey;
        public string Name { get; } = name;
        public string? Type { get; } = type;
        public Dictionary<string, HashSet<string>> ImageKeysByVersion { get; } = new(StringComparer.Ordinal);
        public HashSet<Purl> Purls { get; } = [];
        public HashSet<string> Licenses { get; } = new(StringComparer.Ordinal);

        public void Add(string version, string imageKey, Purl? purl)
        {
            if (!ImageKeysByVersion.TryGetValue(version, out HashSet<string>? imageKeys))
            {
                imageKeys = [];
                ImageKeysByVersion[version] = imageKeys;
            }

            imageKeys.Add(imageKey);

            if (purl is { } value)
            {
                Purls.Add(value);
            }
        }
    }

    // ------------------------------------------------------------------------
    // license
    // ------------------------------------------------------------------------

    public async Task<HomeWhereIsLicenseDto> FindLicense(string query, HomeFilter filter, CancellationToken ctx = default)
    {
        string q = query.Trim();

        string? reason = !sources.HasVulnerabilityReports ? NoVulnerabilityReports
            : !sources.HasSbom ? NoSbom
            : null;

        if (reason is not null)
        {
            return new HomeWhereIsLicenseDto(false, reason, q, null, 0, []);
        }

        if (q.Length < MinLicenseQueryLength)
        {
            return new HomeWhereIsLicenseDto(true, null, q, TooShort(MinLicenseQueryLength), 0, []);
        }

        ActiveVulnerabilityTable table = await GetTable(ctx);
        HomeScope scope = new(table, filter.Namespaces);

        (IReadOnlyDictionary<Digest, ImagePurls> imagePurls, int imagesWithoutSbom) = await GetImagePurls(table, scope, ctx);

        Dictionary<Purl, HashSet<string>> imageKeysByPurl = [];

        foreach ((Digest digest, ImagePurls purls) in imagePurls)
        {
            foreach (Purl purl in purls.Purls)
            {
                if (!imageKeysByPurl.TryGetValue(purl, out HashSet<string>? imageKeys))
                {
                    imageKeys = [];
                    imageKeysByPurl[purl] = imageKeys;
                }

                imageKeys.Add(digest.Value);
            }
        }

        IReadOnlyDictionary<Purl, PurlDetail> purlDetails = await sources.PurlDetails!.GetDetails(imageKeysByPurl.Keys, ctx);

        // license -> package key -> its versions and images
        Dictionary<string, Dictionary<string, PackageMatch>> byLicense = new(StringComparer.Ordinal);

        foreach ((Purl purl, PurlDetail detail) in purlDetails)
        {
            foreach (string license in detail.Licenses.Select(ToLicenseLabel))
            {
                if (!license.Contains(q, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!byLicense.TryGetValue(license, out Dictionary<string, PackageMatch>? packages))
                {
                    packages = new Dictionary<string, PackageMatch>(StringComparer.Ordinal);
                    byLicense[license] = packages;
                }

                Purl.Components components = purl.Parse();
                PackageMatch match = GetMatch(
                    packages,
                    PackageKey.FromPurl(purl)?.Value ?? purl.Value,
                    components.Name ?? purl.Value,
                    components.Type);

                foreach (string imageKey in imageKeysByPurl[purl])
                {
                    match.Add(components.Version ?? string.Empty, imageKey, purl);
                }
            }
        }

        List<HomeWhereIsLicenseItemDto> items =
        [
            .. byLicense
                .Select(license =>
                {
                    HomeReach reach = scope.GetReach(license.Value.Values.SelectMany(x => x.ImageKeysByVersion.Values.SelectMany(y => y)));

                    return new HomeWhereIsLicenseItemDto(
                        License: license.Key,
                        TotalPackages: license.Value.Count,
                        Images: reach.ImageKeys.Count,
                        Namespaces: reach.Namespaces.Count,
                        NamespaceNames: ToNamespaceNames(reach.Namespaces),
                        Packages:
                        [
                            .. license.Value.Values
                                .Select(match =>
                                {
                                    HomeReach packageReach = scope.GetReach(match.ImageKeysByVersion.Values.SelectMany(x => x));

                                    return new HomeWhereIsLicensePackageDto(
                                        PackageKey: match.PackageKey,
                                        Name: match.Name,
                                        Versions: [.. match.ImageKeysByVersion.Keys.Order(StringComparer.Ordinal),],
                                        Images: packageReach.ImageKeys.Count,
                                        Namespaces: packageReach.Namespaces.Count);
                                })
                                .OrderByDescending(x => x.Images)
                                .ThenBy(x => x.PackageKey, StringComparer.Ordinal)
                                .Take(MaxPackagesPerLicense),
                        ]);
                })
                .OrderByDescending(x => string.Equals(x.License, q, StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(x => x.Images)
                .ThenBy(x => x.License, StringComparer.Ordinal),
        ];

        string? note = WithoutSbomNote(imagesWithoutSbom);

        return new HomeWhereIsLicenseDto(true, null, q, note, items.Count, [.. items.Take(filter.Limit),]);
    }

    // the license as stored: its SPDX id, else its name (see PurlDetail.Create)
    private static string ToLicenseLabel(License license) => license.Id ?? license.Name ?? string.Empty;

    // the sbom purls of the running images in scope, and how many of them have no sbom yet
    private async Task<(IReadOnlyDictionary<Digest, ImagePurls> ImagePurls, int ImagesWithoutSbom)> GetImagePurls(
        ActiveVulnerabilityTable table,
        HomeScope scope,
        CancellationToken ctx)
    {
        Digest[] digests =
        [
            .. table.Images.Values
                .Where(x => x.Digest is not null && scope.Contains(x))
                .Select(x => x.Digest!.Value),
        ];

        IReadOnlyDictionary<Digest, ImagePurls> imagePurls = await sources.ImagePurls!.GetDetails(digests, ctx);

        return (imagePurls, digests.Length - imagePurls.Count);
    }

    // ------------------------------------------------------------------------
    // check
    // ------------------------------------------------------------------------

    public async Task<HomeWhereIsCheckDto> FindCheck(string query, HomeFilter filter, CancellationToken ctx = default)
    {
        string q = query.Trim();

        if (!sources.HasSecurityAssessmentReports)
        {
            return new HomeWhereIsCheckDto(false, NoSecurityAssessments, q, null, 0, []);
        }

        if (q.Length < MinQueryLength)
        {
            return new HomeWhereIsCheckDto(true, null, q, TooShort(MinQueryLength), 0, []);
        }

        // the scope works on the reports here, the table is not needed
        HomeScope scope = new(EmptyTable, filter.Namespaces);
        string normalizedQuery = NormalizeCheckId(q);

        List<(HomeWhereIsCheckItemDto Item, bool IsIdMatch)> matches =
        [
            .. (await assessmentReportsLoader.Load(ctx))
                .Where(report => scope.Contains(report.NamespaceName, report.IsClusterScoped))
                .SelectMany(report => report.Checks.Select(check => (Report: report, Check: check)))
                .Where(x => IsCheckMatch(x.Check, q, normalizedQuery))
                .GroupBy(x => (x.Report.Family, x.Check.CheckId))
                .Select(group =>
                {
                    Check check = group.First().Check;
                    HomeAssessmentReport[] reports = [.. group.Select(x => x.Report).DistinctBy(x => x.Uid),];
                    NamespaceName[] namespaces =
                        [.. reports.Where(x => !x.IsClusterScoped).Select(x => x.NamespaceName).Distinct(),];

                    HomeWhereIsCheckItemDto item = new(
                        Family: group.Key.Family,
                        CheckId: group.Key.CheckId.Value,
                        Title: check.Title.Value,
                        SeverityId: check.Severity.Rank,
                        Remediation: check.Remediation.Value,
                        Resources: reports.Length,
                        Namespaces: namespaces.Length,
                        NamespaceNames: ToNamespaceNames(namespaces),
                        SampleResources:
                        [
                            .. reports
                                .Select(ToResource)
                                .OrderBy(x => x.NamespaceName, StringComparer.Ordinal)
                                .ThenBy(x => x.ResourceName, StringComparer.Ordinal)
                                .Take(MaxSampleResources),
                        ]);

                    return (item, NormalizeCheckId(group.Key.CheckId.Value) == normalizedQuery);
                }),
        ];

        List<HomeWhereIsCheckItemDto> items =
        [
            .. matches
                .OrderByDescending(x => x.IsIdMatch)
                .ThenBy(x => x.Item.SeverityId)
                .ThenByDescending(x => x.Item.Resources)
                .ThenBy(x => x.Item.CheckId, StringComparer.Ordinal)
                .Select(x => x.Item),
        ];

        return new HomeWhereIsCheckDto(true, null, q, null, items.Count, [.. items.Take(filter.Limit),]);
    }

    private static bool IsCheckMatch(Check check, string query, string normalizedQuery) =>
        NormalizeCheckId(check.CheckId.Value) == normalizedQuery ||
        check.CheckId.Value.Contains(query, StringComparison.OrdinalIgnoreCase) ||
        check.Title.Value.Contains(query, StringComparison.OrdinalIgnoreCase);

    // KSV014, KSV-0014, AVD-KSV-0014 -> KSV14: Trivy reports the same check with different id formats
    private static string NormalizeCheckId(string checkId)
    {
        string value = checkId.Trim().ToUpperInvariant();

        if (value.StartsWith("AVD-", StringComparison.Ordinal))
        {
            value = value[4..];
        }

        value = value.Replace("-", string.Empty, StringComparison.Ordinal);

        Match match = CheckIdRegex().Match(value);

        return match.Success && int.TryParse(match.Groups[2].Value, out int number)
            ? $"{match.Groups[1].Value}{number}"
            : value;
    }

    [GeneratedRegex("^([A-Z]+)([0-9]+)$")]
    private static partial Regex CheckIdRegex();

    // the assessed resource: the owner of the report when there is one, else the report
    private static HomeWhereIsCheckResourceDto ToResource(HomeAssessmentReport report)
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

    // ------------------------------------------------------------------------
    // helpers
    // ------------------------------------------------------------------------

    private static readonly ActiveVulnerabilityTable EmptyTable = new() { Rows = [], Images = new Dictionary<string, ActiveImage>(), };

    private Task<ActiveVulnerabilityTable> GetTable(CancellationToken ctx) =>
        tableCache.GetOrBuild(tableBuilder.Build, ctx);

    private static string TooShort(int minLength) => $"Type at least {minLength} characters.";

    private static string? WithoutSbomNote(int imagesWithoutSbom) =>
        imagesWithoutSbom switch
        {
            0 => null,
            1 => "1 running image has no SBOM report yet, it is not searched.",
            _ => $"{imagesWithoutSbom} running images have no SBOM report yet, they are not searched.",
        };
}
