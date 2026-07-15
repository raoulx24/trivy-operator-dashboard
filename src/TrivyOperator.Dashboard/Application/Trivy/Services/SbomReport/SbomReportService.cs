using k8s.Models;
using Microsoft.Extensions.Options;
using System.IO.Compression;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using TrivyOperator.Dashboard.Application.Trivy.Models;
using TrivyOperator.Dashboard.Application.Trivy.Services.Options;
using TrivyOperator.Dashboard.Application.Trivy.Services.SbomReport.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy;
using TrivyOperator.Dashboard.Domain.TrivyOld;
using TrivyOperator.Dashboard.Domain.TrivyOld.CustomResources.Abstractions;
using TrivyOperator.Dashboard.Domain.TrivyOld.SbomReport;
using TrivyOperator.Dashboard.Domain.TrivyOld.VulnerabilityReport;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemoryOld.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.K8s.Services.Abstractions;

namespace TrivyOperator.Dashboard.Application.Trivy.Services.SbomReport;

public class SbomReportService(
    IConcurrentDictionaryCache<SbomReportCr> cache,
    IConcurrentDictionaryCache<VulnerabilityReportCr> vrCache,
    INamespacedResourceWatchService<SbomReportCr, CustomResourceList<SbomReportCr>> service,
    IOptions<FileExportOptions> fileExportOptions,
    ILogger<SbomReportService> logger
) : ISbomReportService
{
    private static readonly Regex InvalidFileNameCharsRegex = new(
        $"[{Regex.Escape(new string(Path.GetInvalidFileNameChars()))}]",
        RegexOptions.Compiled
    );

    public Task<IEnumerable<SbomReportDto>> GetSbomReportDtos(string? namespaceName = null)
    {
        IEnumerable<SbomReportCr> cachedValues =
        [
            .. cache.Where(kvp => string.IsNullOrEmpty(namespaceName) || kvp.Key == namespaceName)
                .SelectMany(kvp => kvp.Value.Values),
        ];
        IEnumerable<SbomReportDto> dtos = cachedValues.Select(cr => cr.ToSbomReportDto());

        return Task.FromResult(dtos);
    }

    public Task<IEnumerable<SbomReportImageDto>> GetSbomReportImageDtos(
        string? digest = null,
        string? namespaceName = null
    )
    {
        IEnumerable<SbomReportCr> cachedValues =
        [
            .. cache.Where(kvp => string.IsNullOrEmpty(namespaceName) || kvp.Key == namespaceName)
                .SelectMany(kvp => kvp.Value.Values),
        ];
        IEnumerable<VulnerabilityReportCr> cachedVrValues =
        [
            .. vrCache.Where(kvp => string.IsNullOrEmpty(namespaceName) || kvp.Key == namespaceName)
                .SelectMany(kvp => kvp.Value.Values),
        ];
        var vrDigests = cachedVrValues
            .Where(vr => string.IsNullOrEmpty(digest) || vr.Report?.Artifact?.Digest == digest)
            .GroupBy(vr => new
                {
                    ImageDigest = vr.Report?.Artifact?.Digest ?? string.Empty,
                    ResourceNamespace = vr.Metadata.NamespaceProperty,
                }
            )
            .Select(group => new
                {
                    group.Key.ImageDigest,
                    group.Key.ResourceNamespace,
                    CriticalCount = group.FirstOrDefault()?.Report?.Summary?.CriticalCount ?? -1,
                    HighCount = group.FirstOrDefault()?.Report?.Summary?.HighCount ?? -1,
                    MediumCount = group.FirstOrDefault()?.Report?.Summary?.MediumCount ?? -1,
                    LowCount = group.FirstOrDefault()?.Report?.Summary?.LowCount ?? -1,
                    UnknownCount = group.FirstOrDefault()?.Report?.Summary?.UnknownCount ?? -1,
                }
            );
        IEnumerable<SbomReportImageDto> dtos = cachedValues
            .Where(vr => string.IsNullOrEmpty(digest) || vr.Report?.Artifact?.Digest == digest)
            .GroupBy(sbom => new ImageGroupKey(sbom.Report?.Artifact?.Digest, sbom.Namespace()))
            .Select(group => group.ToSbomReportImageDto())
            .GroupJoin(
                vrDigests,
                dto => new
                {
                    dto.ImageDigest,
                    dto.ResourceNamespace,
                },
                vr => new
                {
                    vr.ImageDigest,
                    vr.ResourceNamespace,
                },
                (dto, vrMatches) =>
                {
                    var vr = vrMatches.FirstOrDefault();
                    dto.HasVulnerabilities = vr != null;
                    dto.CriticalCount = vr?.CriticalCount ?? -1;
                    dto.HighCount = vr?.HighCount ?? -1;
                    dto.MediumCount = vr?.MediumCount ?? -1;
                    dto.LowCount = vr?.LowCount ?? -1;
                    dto.UnknownCount = vr?.UnknownCount ?? -1;

                    return dto;
                }
            );

        return Task.FromResult(dtos);
    }

    public Task<IEnumerable<SbomReportImageMinimalDto>> GetSbomReportImageMinimalDtos(string? namespaceName = null)
    {
        IEnumerable<SbomReportCr> cachedValues =
        [
            .. cache.Where(kvp => string.IsNullOrEmpty(namespaceName) || kvp.Key == namespaceName)
                .SelectMany(kvp => kvp.Value.Values),
        ];
        IEnumerable<VulnerabilityReportCr> cachedVrValues =
        [
            .. vrCache.Where(kvp => string.IsNullOrEmpty(namespaceName) || kvp.Key == namespaceName)
                .SelectMany(kvp => kvp.Value.Values),
        ];
        var vrDigests = cachedVrValues.GroupBy(vr => new
                {
                    ImageDigest = vr.Report?.Artifact?.Digest ?? string.Empty,
                    ResourceNamespace = vr.Metadata.NamespaceProperty,
                }
            )
            .Select(group => new
                {
                    group.Key.ImageDigest,
                    group.Key.ResourceNamespace,
                    CriticalCount = group.FirstOrDefault()?.Report?.Summary?.CriticalCount ?? -1,
                    HighCount = group.FirstOrDefault()?.Report?.Summary?.HighCount ?? -1,
                    MediumCount = group.FirstOrDefault()?.Report?.Summary?.MediumCount ?? -1,
                    LowCount = group.FirstOrDefault()?.Report?.Summary?.LowCount ?? -1,
                    UnknownCount = group.FirstOrDefault()?.Report?.Summary?.UnknownCount ?? -1,
                }
            );
        IEnumerable<SbomReportImageMinimalDto> dtos = cachedValues
            .GroupBy(sbom => new ImageGroupKey(sbom.Report?.Artifact?.Digest, sbom.Namespace()))
            .Select(group => group.ToSbomReportImageMinimalDto())
            .GroupJoin(
                vrDigests,
                dto => new
                {
                    dto.ImageDigest,
                    dto.ResourceNamespace,
                },
                vr => new
                {
                    vr.ImageDigest,
                    vr.ResourceNamespace,
                },
                (dto, vrMatches) =>
                {
                    var vr = vrMatches.FirstOrDefault();
                    dto.HasVulnerabilities = vr != null;
                    dto.CriticalCount = vr?.CriticalCount ?? -1;
                    dto.HighCount = vr?.HighCount ?? -1;
                    dto.MediumCount = vr?.MediumCount ?? -1;
                    dto.LowCount = vr?.LowCount ?? -1;
                    dto.UnknownCount = vr?.UnknownCount ?? -1;
                    return dto;
                }
            );
        return Task.FromResult(dtos);
    }

    public async Task<SbomReportDto?> GetFullSbomReportDtoByUid(string uid)
    {
        string[] namespaceNames = [.. cache.Where(x => !x.Value.IsEmpty).Select(x => x.Key),];

        foreach (string namespaceName in namespaceNames)
        {
            SbomReportDto? sr = await GetFullSbomReportDtoByUidNamespace(uid, namespaceName);
            if (sr != null)
            {
                return sr;
            }
        }

        return null;
    }

    public async Task<SbomReportDto?> GetFullSbomReportDtoByUidNamespace(string uid, string namespaceName)
    {
        IEnumerable<SbomReportCr> cachedValues = [.. cache[namespaceName].Select(kvp => kvp.Value) ?? [],];

        SbomReportCr? sr = cachedValues.FirstOrDefault(x => x.Metadata.Uid == uid);
        if (sr != null)
        {
            try
            {
                SbomReportDto sbomReportDto =
                    (await service.GetResource(sr.Metadata.Name, sr.Metadata.NamespaceProperty))
                    .ToSbomReportDto();
                SetVulnerabilityReportStatistics(sbomReportDto);
                return sbomReportDto;
            }
            catch
            {
                // ignored
            }
        }

        return null;
    }

    public async Task<SbomReportDto?> GetFullSbomReportDtoByDigestNamespace(string digest, string namespaceName)
    {
        IEnumerable<SbomReportCr> cachedValues = [.. cache[namespaceName].Select(kvp => kvp.Value) ?? [],];

        SbomReportCr? x = cachedValues.Where(x => x.Report?.Artifact?.Digest == digest)
            .Aggregate(
                (SbomReportCr?)null,
                (max, current) => max == null || current?.Metadata.CreationTimestamp > max.Metadata.CreationTimestamp
                    ? current : max
            );
        if (x != null)
        {
            return await GetFullSbomReportDtoByUidNamespace(x.Metadata.Uid, namespaceName);
        }

        return null;
    }

    public async Task<CycloneDxBom?> GetCycloneDxBomByDigestNamespace(
        string digest,
        string namespaceName,
        IEnumerable<SbomReportCr>? cachedValues = null
    )
    {
        cachedValues ??= [.. cache[namespaceName].Select(kvp => kvp.Value) ?? [],];

        SbomReportCr? sr = cachedValues.Where(x => x.Report?.Artifact?.Digest == digest)
            .Aggregate(
                (SbomReportCr?)null,
                (max, current) => max == null || current?.Metadata.CreationTimestamp > max.Metadata.CreationTimestamp
                    ? current : max
            );
        if (sr != null)
        {
            try
            {
                CycloneDxBom cycloneDx =
                    (await service.GetResource(sr.Metadata.Name, sr.Metadata.NamespaceProperty)).ToCycloneDx();
                return cycloneDx;
            }
            catch
            {
                // ignored
            }
        }

        return null;
    }

    public async Task<SpdxBom?> GetSpdxBomByDigestNamespace(string digest, string namespaceName)
    {
        IEnumerable<SbomReportCr> cachedValues = [.. cache[namespaceName].Select(kvp => kvp.Value) ?? [],];

        SbomReportCr? sr = cachedValues.Where(x => x.Report?.Artifact?.Digest == digest)
            .Aggregate(
                (SbomReportCr?)null,
                (max, current) => max == null || current?.Metadata.CreationTimestamp > max.Metadata.CreationTimestamp
                    ? current : max
            );
        if (sr != null)
        {
            try
            {
                SpdxBom spdx =
                    (await service.GetResource(sr.Metadata.Name, sr.Metadata.NamespaceProperty)).ToSpdx();
                return spdx;
            }
            catch
            {
                // ignored
            }
        }

        return null;
    }

    public async Task<string> CreateCycloneDxExportZipFile(SbomReportExportDto[] exportSboms, string fileType = "json")
    {
        try
        {
            IEnumerable<SbomReportCr> cachedValues = [.. cache.SelectMany(kvp => kvp.Value.Values),];

            HashSet<(string NamespaceName, string Digest)> requestedSboms = exportSboms
                .Select(x => (x.NamespaceName, x.Digest))
                .ToHashSet();

            Dictionary<string, SbomReportCr> filteredCachedValues = [];

            foreach (SbomReportCr sbomReportCr in cachedValues)
            {
                string ns = sbomReportCr.Metadata.NamespaceProperty;
                string? digest = sbomReportCr.Report?.Artifact?.Digest;
                if (digest is null)
                {
                    continue;
                }

                // Only process if this (namespace, digest) was requested
                if (!requestedSboms.Contains((ns, digest)))
                {
                    continue;
                }

                // Keep the newest report per digest
                if (!filteredCachedValues.TryGetValue(digest, out SbomReportCr? existing) ||
                    sbomReportCr.Metadata.CreationTimestamp > existing.Metadata.CreationTimestamp)
                {
                    filteredCachedValues[digest] = sbomReportCr;
                }
            }

            Guid fileNameGuid = Guid.NewGuid();
            string zipFileName = Path.Combine(fileExportOptions.Value.TempFolder, $"{fileNameGuid}_sbom.zip");

            await using FileStream zipFileStream = new(zipFileName, FileMode.Create);
            using ZipArchive archive = new(zipFileStream, ZipArchiveMode.Create);
            foreach (SbomReportExportDto exportSbom in exportSboms)
            {
                CycloneDxBom? cycloneDxBom = await GetCycloneDxBomByDigestNamespace(
                    exportSbom.Digest,
                    exportSbom.NamespaceName,
                    filteredCachedValues.Where(x => x.Value != null).Select(x => x.Value)
                );

                if (cycloneDxBom == null)
                {
                    logger.LogWarning(
                        "CycloneDxBom not found for {Digest} in {NamespaceName}",
                        exportSbom.Digest,
                        exportSbom.NamespaceName
                    );
                    continue;
                }

                string imageName = cycloneDxBom.Metadata?.Component?.Name ?? string.Empty;
                string imageVersion = cycloneDxBom.Metadata?.Component?.Version ?? string.Empty;
                string fileExtension = fileType.ToLower() == "json" ? "json" : "xml";
                string fileName = InvalidFileNameCharsRegex.Replace(
                    $"{exportSbom.NamespaceName}_{imageName}_{imageVersion}_{exportSbom.Digest}.{fileExtension}",
                    "_"
                );
                await using Stream stream = archive.CreateEntry(fileName).Open();
                if (fileType == "json")
                {
                    JsonSerializer.Serialize(stream, cycloneDxBom);
                }
                else
                {
                    XmlSerializer serializer = new(cycloneDxBom.GetType());
                    serializer.Serialize(stream, cycloneDxBom);
                }
            }

            return zipFileName;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating zip file - {exceptionMessage}", ex.Message);
            return string.Empty;
        }
    }

    public void CleanupFile(string fileName)
    {
        try
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting file - {exceptionMessage}", ex.Message);
        }
    }

    public Task<IEnumerable<string>> GetActiveNamespaces() =>
        Task.FromResult<IEnumerable<string>>([.. cache.Where(x => !x.Value.IsEmpty).Select(x => x.Key),]);

    public Task<IEnumerable<SbomReportImageResourceDto>> GetSbomReportImageResourceDtosByDigestAndNamespace(
        string digest,
        string namespaceName
    )
    {
        IEnumerable<SbomReportCr> cachedValues = [.. cache[namespaceName].Select(kvp => kvp.Value) ?? [],];
        IEnumerable<SbomReportImageResourceDto> dtos = cachedValues.Where(x => x.Report?.Artifact?.Digest == digest)
            .Select(x => x.ToSbomReportImageResourceDto());
        return Task.FromResult(dtos);
    }

    private void SetVulnerabilityReportStatistics(SbomReportDto sbomReportDto)
    {
        IEnumerable<VulnerabilityReportCr> cachedVrValues =
            [.. vrCache[sbomReportDto.ResourceNamespace].Select(kvp => kvp.Value) ?? [],];

        VulnerabilityReportCr? vr = cachedVrValues.Where(x => x.Report?.Artifact?.Digest == sbomReportDto.ImageDigest)
            .Aggregate(
                (VulnerabilityReportCr?)null,
                (max, current) => max == null || current?.Metadata.CreationTimestamp > max.Metadata.CreationTimestamp
                    ? current : max
            );

        if (vr != null)
        {
            var result = vr.Report?.Vulnerabilities?.GroupBy(vrd => new
                                 {
                                     vrd.PackagePurl,
                                 }
                             )
                             .Select(g => new
                                 {
                                     g.Key.PackagePurl,
                                     CriticalCount = g.Count(vrd => vrd.Severity == TrivySeverity.CRITICAL),
                                     HighCount = g.Count(vrd => vrd.Severity == TrivySeverity.HIGH),
                                     MediumCount = g.Count(vrd => vrd.Severity == TrivySeverity.MEDIUM),
                                     LowCount = g.Count(vrd => vrd.Severity == TrivySeverity.LOW),
                                     UnknownCount = g.Count(vrd => vrd.Severity == TrivySeverity.UNKNOWN),
                                 }
                             )
                             .ToArray() ??
                         [];

            // build lookup once - and then search dict o(1)
            Dictionary<string, IGrouping<string, SbomReportDetailDto>>? lookup = sbomReportDto.Details?
                .GroupBy(x => x.Purl)
                .ToDictionary(g => g.Key, g => g);

            foreach (var item in result)
            {
                if (lookup != null && lookup.TryGetValue(item.PackagePurl, out IGrouping<string, SbomReportDetailDto>? matches))
                {
                    foreach (SbomReportDetailDto sbomReportDetailDto in matches)
                    {
                        sbomReportDetailDto.CriticalCount = item.CriticalCount;
                        sbomReportDetailDto.HighCount = item.HighCount;
                        sbomReportDetailDto.MediumCount = item.MediumCount;
                        sbomReportDetailDto.LowCount = item.LowCount;
                        sbomReportDetailDto.UnknownCount = item.UnknownCount;
                    }
                }
                else
                {
                    logger.LogWarning(
                        "SbomReportDetailDto not found for package purl {PackagePurl}, in {SbomResourceName} - {NamespaceName}",
                        item.PackagePurl,
                        sbomReportDto.ResourceName,
                        sbomReportDto.ResourceNamespace
                    );
                }
            }

        }
    }
}
