using Microsoft.Extensions.Options;
using System.IO.Compression;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Mappers;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Options;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.SbomReports.Abstractions;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Services.SbomReports;

public class SbomReportService(
    IResourceProvider<SbomReport,Digest> resourceProvider,
    IResourceProvider<VulnerabilityReport, Digest> vrResourceProvider,
    IOptions<FileExportOptions> fileExportOptions,
    ILogger<SbomReportService> logger
) : ISbomReportService
{
    private static readonly Regex InvalidFileNameCharsRegex = new(
        $"[{Regex.Escape(new string(Path.GetInvalidFileNameChars()))}]",
        RegexOptions.Compiled
    );

    public async Task<IEnumerable<SbomReportImageMinimalDto>> GetSbomReportImageMinimalDtos(CancellationToken ctx = default)
    {
        IReadOnlyList<SbomReport> resourceSummaries = await resourceProvider.GetResourceSummaries(ctx);
        HashSet<Digest> vrDigests = [.. await vrResourceProvider.GetResourceIds(ctx),];

        return resourceSummaries
            .SelectMany(x => x.ToMinimalDto(vrDigests.Contains(x.ImageDigest)));
    }

    public async Task<SbomReportImageDto?> GetFullSbomReportImageDtoByDigest(
        string digest,
        CancellationToken ctx = default
    )
    {
        Digest key = new(digest);

        SbomReport? sbomReport = await resourceProvider.GetResource(key, ctx);

        if (sbomReport is null)
            return null;
        
        VulnerabilityReport? vulnerabilityReport = await vrResourceProvider.GetResource(key, ctx);
        Dictionary<Purl, SeverityCounters> severities =
            vulnerabilityReport?.Vulnerabilities
                .GroupBy(v => v.ScannedPackage.Purl)
                .ToDictionary(
                    g => g.Key,
                    g => new SeverityCounters(g.Select(v => v.Severity))
                )
            ?? [];

        return sbomReport.ToImageDto(severities);
    }


    public async Task<CycloneDxBom?> GetCycloneDxBomByDigest(string digest, CancellationToken ctx = default)
    {
        return await GetCycloneDxBomByDigest(new Digest(digest), ctx);
    }

    private async Task<CycloneDxBom?> GetCycloneDxBomByDigest(Digest digest, CancellationToken ctx = default)
    {
        SbomReport? report = await resourceProvider.GetResource(digest, ctx);

        return report?.ToCycloneDx();
    }

    public async Task<SpdxBom?> GetSpdxBomByDigest(string digest, CancellationToken ctx = default)
    {
        SbomReport? report = await resourceProvider.GetResource(new Digest(digest), ctx);

        return report?.ToSpdx();
    }

    public async Task<SbomExportFileDto?> CreateCycloneDxExportZipFile(
        SbomReportExportDto[] exportSboms,
        string fileType = "json",
        CancellationToken ctx = default)
    {
        FileStream? zipFileStream = null;

        HashSet<Digest> filteredDigests = [.. exportSboms.Select(static x => new Digest(x.Digest)),];

        string zipFileName = Path.Combine(
            fileExportOptions.Value.TempFolder,
            $"{Guid.NewGuid()}_sbom.zip"
        );
        
        try
        {
            zipFileStream = new FileStream(
                zipFileName,
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.None,
                bufferSize: 64 * 1024,
                FileOptions.Asynchronous |
                FileOptions.SequentialScan |
                FileOptions.DeleteOnClose
            );

            await using (ZipArchive archive = new(
                             zipFileStream,
                             ZipArchiveMode.Create,
                             leaveOpen: true))
            {
                foreach (Digest digest in filteredDigests)
                {
                    CycloneDxBom? cycloneDxBom = await GetCycloneDxBomByDigest(digest, ctx);

                    if (cycloneDxBom == null)
                    {
                        logger.LogWarning(
                            "CycloneDxBom not found for {Digest}",
                            digest
                        );

                        continue;
                    }

                    string imageName =
                        cycloneDxBom.Metadata?.Component?.Name ?? string.Empty;

                    string imageVersion =
                        cycloneDxBom.Metadata?.Component?.Version ?? string.Empty;

                    string fileExtension =
                        fileType.Equals("json", StringComparison.OrdinalIgnoreCase)
                            ? "json"
                            : "xml";

                    string fileName = InvalidFileNameCharsRegex.Replace(
                        $"{imageName}_{imageVersion}_{digest}.{fileExtension}",
                        "_"
                    );

                    await using Stream stream = await archive.CreateEntry(fileName).OpenAsync(ctx);

                    if (fileType.Equals("json", StringComparison.OrdinalIgnoreCase))
                    {
                        await JsonSerializer.SerializeAsync(stream, cycloneDxBom, cancellationToken: ctx);
                    }
                    else
                    {
                        XmlSerializer serializer = new(cycloneDxBom.GetType());
                        serializer.Serialize(stream, cycloneDxBom);
                    }
                }
            }

            zipFileStream.Position = 0;

            return new SbomExportFileDto(
                Stream: zipFileStream,
                FileName: Path.GetFileName(zipFileName)
            );
        }
        catch (Exception ex)
        {
            if (zipFileStream is not null)
                await zipFileStream.DisposeAsync();

            logger.LogError(
                ex,
                "Error creating zip file - {exceptionMessage}",
                ex.Message
            );

            return null;
        }
    }
}
