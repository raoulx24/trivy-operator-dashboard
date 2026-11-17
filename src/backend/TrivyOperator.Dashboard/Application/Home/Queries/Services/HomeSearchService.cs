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
            .. VulnerabilityGroups(table, scope, id => id.StartsWith(q, StringComparison.OrdinalIgnoreCase))
                .Select(group =>
                {
                    ActiveVulnerability first = group.First();
                    HomeReach reach = scope.GetReach(group.SelectMany(x => x.ImageKeys));

                    return new HomeWhereIsVulnerabilityItemDto(
                        VulnerabilityId: group.Key.Value,
                        Title: first.Title,
                        SeverityId: group.Min(x => x.Severity.Rank),
                        IsKnownExploited: first.IsKnownExploited,
                        PackagesBySeverity:
                        [
                            .. group
                                .GroupBy(x => x.Severity.Rank)
                                .OrderBy(x => x.Key)
                                .Select(x => new HomeWhereIsVulnerabilitySeverityDto(x.Key, x.Count())),
                        ],
                        Packages: group.Count(),
                        FixablePackages: group.Count(x => x.HasFix),
                        Images: reach.ImageKeys.Count,
                        Namespaces: reach.Namespaces.Count,
                        Workloads: reach.Workloads.Count);
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

    // everything about one id (exact, case-insensitive), with all its images and namespaces
    public async Task<HomeWhereIsVulnerabilityDetailDto> FindVulnerabilityDetailed(
        string vulnerabilityId,
        HomeFilter filter,
        CancellationToken ctx = default)
    {
        string id = vulnerabilityId.Trim();

        if (!sources.HasVulnerabilityReports)
        {
            return new HomeWhereIsVulnerabilityDetailDto(false, NoVulnerabilityReports, id, null, null);
        }

        ActiveVulnerabilityTable table = await GetTable(ctx);
        HomeScope scope = new(table, filter.Namespaces);

        IGrouping<VulnerabilityId, ActiveVulnerability>? group = VulnerabilityGroups(
                table,
                scope,
                x => string.Equals(x, id, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault();

        if (group is null)
        {
            return new HomeWhereIsVulnerabilityDetailDto(
                true,
                null,
                id,
                "Not found. Trivy may report it under another id (e.g. a GHSA or a distro advisory); aliases are not known here.",
                null);
        }

        ActiveVulnerability first = group.First();
        HomeReach reach = scope.GetReach(group.SelectMany(x => x.ImageKeys));

        ImageRelations relations = BuildImageRelations(reach, scope, table);

        HomeWhereIsVulnerabilityDetailItemDto item = new(
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
                        Images: scope.GetReach(row).ImageKeys.Count,
                        ImageIndexes:
                        [
                            .. scope.GetReach(row).ImageKeys
                                .Select(key => relations.ImageIndexes[ToImageName(key, table)])
                                .Distinct()
                                .Order(),
                        ]))
                    .OrderBy(x => x.SeverityId)
                    .ThenByDescending(x => x.Images),
            ],
            Images: reach.ImageKeys.Count,
            Namespaces: reach.Namespaces.Count,
            Workloads: reach.Workloads.Count,
            NamespaceNames: relations.NamespaceNames,
            ImageNames: relations.ImageNames,
            ImageNamespaceIndexes: relations.ImageNamespaceIndexes);

        return new HomeWhereIsVulnerabilityDetailDto(true, null, id, null, item);
    }

    // name -> its index in the list (the names are distinct)
    private static Dictionary<string, int> ToIndexes(IReadOnlyList<string> names) =>
        names.Select((name, index) => (name, index)).ToDictionary(x => x.name, x => x.index);

    // the rows in scope whose id is accepted, by id
    private static IEnumerable<IGrouping<VulnerabilityId, ActiveVulnerability>> VulnerabilityGroups(
        ActiveVulnerabilityTable table,
        HomeScope scope,
        Func<string, bool> acceptsId) =>
        table.Rows
            .Where(x => acceptsId(x.Key.VulnerabilityId.Value) && scope.Contains(x))
            .GroupBy(x => x.Key.VulnerabilityId);

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

    // one item per package version: the packages of the sbom (or, without it, the vulnerable ones) that match
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

        (Dictionary<(string Key, string Version), PackageVersionMatch> matches, string? note) =
            await CollectPackageVersions(table, scope, matcher.Matches, ctx);

        // the active vulnerabilities of a version of a package: the rows of the package found in that version
        ILookup<string, ActiveVulnerability> rowsByPackageKey = table.Rows.ToLookup(x => x.Key.PackageKey.Value, StringComparer.Ordinal);

        List<HomeWhereIsPackageItemDto> items =
        [
            .. matches.Values
                .Select(match =>
                {
                    HomeReach reach = scope.GetReach(match.ImageKeys);
                    ActiveVulnerability[] vulnerable = VulnerabilitiesOf(match, rowsByPackageKey[match.PackageKey]);

                    return new HomeWhereIsPackageItemDto(
                        PackageKey: match.PackageKey,
                        Version: match.Version,
                        Purl: ToPurl(match.PackageKey, match.Version),
                        Name: match.Name,
                        Type: match.Type,
                        Images: reach.ImageKeys.Count,
                        Namespaces: reach.Namespaces.Count,
                        Workloads: reach.Workloads.Count,
                        VulnerabilityIds: vulnerable.Length,
                        MaxSeverityId: vulnerable.Length == 0 ? null : vulnerable.Min(x => x.Severity.Rank));
                })
                .OrderByDescending(x => string.Equals(x.Name, q, StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(x => x.Images)
                .ThenBy(x => x.PackageKey, StringComparer.Ordinal)
                .ThenBy(x => x.Version, StringComparer.Ordinal),
        ];

        return new HomeWhereIsPackageDto(true, null, q, note, source, items.Count, [.. items.Take(filter.Limit),]);
    }

    // one version of a package (exact match on the package key and the version) with all its images and namespaces, its
    // licenses and its vulnerabilities
    public async Task<HomeWhereIsPackageDetailDto> FindPackageDetailed(
        string packageKey,
        string version,
        HomeFilter filter,
        CancellationToken ctx = default)
    {
        string key = packageKey.Trim();
        string versionValue = version.Trim();

        if (!sources.HasVulnerabilityReports)
        {
            return new HomeWhereIsPackageDetailDto(false, NoVulnerabilityReports, key, versionValue, null, null);
        }

        ActiveVulnerabilityTable table = await GetTable(ctx);
        HomeScope scope = new(table, filter.Namespaces);

        (Dictionary<(string Key, string Version), PackageVersionMatch> matches, string? note) =
            await CollectPackageVersions(
                table,
                scope,
                (matchKey, _, matchVersion) =>
                    string.Equals(matchKey, key, StringComparison.Ordinal) &&
                    string.Equals(matchVersion, versionValue, StringComparison.Ordinal),
                ctx);

        if (!matches.TryGetValue((key, versionValue), out PackageVersionMatch? match))
        {
            return new HomeWhereIsPackageDetailDto(
                true,
                null,
                key,
                versionValue,
                "Not found: this version of the package does not run now.",
                null);
        }

        HomeReach reach = scope.GetReach(match.ImageKeys);
        ImageRelations relations = BuildImageRelations(reach, scope, table);

        HomeWhereIsPackageVulnerabilityDto[] vulnerabilities =
        [
            .. VulnerabilitiesOf(match, table.Rows.Where(x => x.Key.PackageKey.Value == key))
                .Select(row => new HomeWhereIsPackageVulnerabilityDto(
                    VulnerabilityId: row.Key.VulnerabilityId.Value,
                    Title: row.Title,
                    SeverityId: row.Severity.Rank,
                    FixedVersion: row.HasFix ? row.FixedVersion!.Value.Value : null,
                    IsKnownExploited: row.IsKnownExploited,
                    ImageIndexes:
                    [
                        .. row.ImageKeysByInstalledVersion[versionValue]
                            .Where(reach.ImageKeys.Contains)
                            .Select(imageKey => relations.ImageIndexes[ToImageName(imageKey, table)])
                            .Distinct()
                            .Order(),
                    ]))
                .OrderBy(x => x.SeverityId)
                .ThenByDescending(x => x.ImageIndexes.Count)
                .ThenBy(x => x.VulnerabilityId, StringComparer.Ordinal),
        ];

        // only the sbom knows the licenses
        IReadOnlyList<string> licenses = [];

        if (sources.HasSbom)
        {
            IReadOnlyDictionary<Purl, PurlDetail> purlDetails = await sources.PurlDetails!.GetDetails(match.Purls, ctx);

            licenses =
            [
                .. match.Purls
                    .Select(x => purlDetails.GetValueOrDefault(x))
                    .OfType<PurlDetail>()
                    .SelectMany(x => x.Licenses)
                    .Select(ToLicenseLabel)
                    .Where(x => x.Length > 0)
                    .Distinct()
                    .Order(StringComparer.Ordinal),
            ];
        }

        HomeWhereIsPackageDetailItemDto item = new(
            PackageKey: key,
            Version: versionValue,
            Purl: ToPurl(key, versionValue),
            Name: match.Name,
            Type: match.Type,
            Licenses: licenses,
            Images: reach.ImageKeys.Count,
            Namespaces: reach.Namespaces.Count,
            Workloads: reach.Workloads.Count,
            MaxSeverityId: vulnerabilities.Length == 0 ? null : vulnerabilities.Min(x => x.SeverityId),
            NamespaceNames: relations.NamespaceNames,
            ImageNames: relations.ImageNames,
            ImageNamespaceIndexes: relations.ImageNamespaceIndexes,
            Vulnerabilities: vulnerabilities);

        return new HomeWhereIsPackageDetailDto(true, null, key, versionValue, note, item);
    }

    // the (package key, version) pairs in scope that the filter accepts, with their images (and purls, from the sbom):
    // the sbom when there is one (all the packages, and how many running images have none yet is in the note), else the
    // vulnerability reports (only the vulnerable packages)
    private async Task<(Dictionary<(string Key, string Version), PackageVersionMatch> Matches, string? Note)>
        CollectPackageVersions(
            ActiveVulnerabilityTable table,
            HomeScope scope,
            Func<string, string, string, bool> accepts,
            CancellationToken ctx)
    {
        Dictionary<(string Key, string Version), PackageVersionMatch> matches = [];

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

                    if (accepts(packageKey, name, version))
                    {
                        GetVersionMatch(matches, packageKey, name, components.Type, version).Add(digest.Value, purl);
                    }
                }
            }

            return (matches, WithoutSbomNote(imagesWithoutSbom));
        }

        foreach (ActiveVulnerability row in table.Rows)
        {
            foreach ((string version, HashSet<string> imageKeys) in row.ImageKeysByInstalledVersion)
            {
                if (!accepts(row.Key.PackageKey.Value, row.PackageName.Value, version))
                {
                    continue;
                }

                foreach (string imageKey in imageKeys.Where(key => scope.Contains(table.Images[key])))
                {
                    GetVersionMatch(
                            matches,
                            row.Key.PackageKey.Value,
                            row.PackageName.Value,
                            new Purl(row.Key.PackageKey.Value).Type,
                            version)
                        .Add(imageKey, null);
                }
            }
        }

        return (matches, "Only the vulnerable packages are searched (SBOM reports are not enabled).");
    }

    private static PackageVersionMatch GetVersionMatch(
        Dictionary<(string Key, string Version), PackageVersionMatch> matches,
        string packageKey,
        string name,
        string? type,
        string version)
    {
        if (!matches.TryGetValue((packageKey, version), out PackageVersionMatch? match))
        {
            match = new PackageVersionMatch(packageKey, name, type, version);
            matches[(packageKey, version)] = match;
        }

        return match;
    }

    // the rows of the package that were found in this version, in the images of the match
    private static ActiveVulnerability[] VulnerabilitiesOf(
        PackageVersionMatch match,
        IEnumerable<ActiveVulnerability> rowsOfPackage) =>
    [
        .. rowsOfPackage
            .Where(row => row.ImageKeysByInstalledVersion.TryGetValue(match.Version, out HashSet<string>? imageKeys) &&
                          imageKeys.Any(match.ImageKeys.Contains)),
    ];

    // the package key with the version before the qualifiers: pkg:deb/ubuntu/libc6?distro=ubuntu-22.04 + 2.35-1
    // -> pkg:deb/ubuntu/libc6@2.35-1?distro=ubuntu-22.04 (what the reports give, not percent-encoded)
    private static string ToPurl(string packageKey, string version)
    {
        if (version.Length == 0)
        {
            return packageKey;
        }

        int qualifiers = packageKey.IndexOf('?', StringComparison.Ordinal);

        return qualifiers < 0
            ? $"{packageKey}@{version}"
            : $"{packageKey[..qualifiers]}@{version}{packageKey[qualifiers..]}";
    }

    private sealed class PackageVersionMatch(string packageKey, string name, string? type, string version)
    {
        public string PackageKey { get; } = packageKey;
        public string Name { get; } = name;
        public string? Type { get; } = type;
        public string Version { get; } = version;
        public HashSet<string> ImageKeys { get; } = new(StringComparer.Ordinal);
        public HashSet<Purl> Purls { get; } = [];

        public void Add(string imageKey, Purl? purl)
        {
            ImageKeys.Add(imageKey);

            if (purl is { } value)
            {
                Purls.Add(value);
            }
        }
    }

    // the namespaces and the images of a reach as the lists of a detailed item: the images by name (the images of one name,
    // with other digests, are one line), and per image the namespaces it runs in, as indexes in the namespaces
    private static ImageRelations BuildImageRelations(HomeReach reach, HomeScope scope, ActiveVulnerabilityTable table)
    {
        IReadOnlyList<string> namespaceNames = ToNamespaceNames(reach.Namespaces, int.MaxValue);
        IReadOnlyList<string> imageNames = ToImageNames(reach.ImageKeys, table, int.MaxValue);
        Dictionary<string, int> namespaceIndexes = ToIndexes(namespaceNames);
        Dictionary<string, int> imageIndexes = ToIndexes(imageNames);

        SortedSet<int>[] namespacesOfImage = [.. imageNames.Select(_ => new SortedSet<int>()),];
        foreach (string imageKey in reach.ImageKeys)
        {
            SortedSet<int> namespacesOfThis = namespacesOfImage[imageIndexes[ToImageName(imageKey, table)]];

            foreach (NamespaceName namespaceName in scope.GetReach(new[] { imageKey, }).Namespaces)
            {
                namespacesOfThis.Add(namespaceIndexes[namespaceName.Value]);
            }
        }

        return new ImageRelations(
            namespaceNames,
            imageNames,
            imageIndexes,
            [.. namespacesOfImage.Select(x => (IReadOnlyList<int>)x.ToArray()),]);
    }

    private sealed record ImageRelations(
        IReadOnlyList<string> NamespaceNames,
        IReadOnlyList<string> ImageNames,
        // image name -> its index in ImageNames
        IReadOnlyDictionary<string, int> ImageIndexes,
        IReadOnlyList<IReadOnlyList<int>> ImageNamespaceIndexes);
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
