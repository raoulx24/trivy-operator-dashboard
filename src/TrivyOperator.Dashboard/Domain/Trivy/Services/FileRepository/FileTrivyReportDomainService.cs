using k8s;
using k8s.Models;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Text.Json;
using TrivyOperator.Dashboard.Domain.Trivy.Services.FileRepository.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Services.FileRepository.Options;
using TrivyOperator.Dashboard.Domain.Utils;

namespace TrivyOperator.Dashboard.Domain.Trivy.Services.FileRepository;

public class FileTrivyReportDomainService<TTrivyReport>(
    IFolderNameFactory folderNameFactory,
    IOptions<FileRepositoryOptions> options,
    ILogger<FileTrivyReportDomainService<TTrivyReport>> logger) : IFileTrivyReportDomainService<TTrivyReport>
    where TTrivyReport : IKubernetesObject<V1ObjectMeta>, IMetadata<V1ObjectMeta>
{
    public async Task<IList<TTrivyReport>> GetAllReportsAsync(
    CancellationToken? cancellationToken = null)
    {
        CancellationToken ct = cancellationToken ?? CancellationToken.None;

        string folder = folderNameFactory.Get<TTrivyReport>();
        string fullPath = Path.Combine(options.Value.BasePath, folder);

        if (!Directory.Exists(fullPath))
            return [];

        ConcurrentBag<TTrivyReport> results = new ConcurrentBag<TTrivyReport>();
        JsonSerializerOptions jsonOptions = new JsonSerializerOptions();
        JsonUtils.ConfigureJsonSerializerOptions(jsonOptions);

        IEnumerable<string> files = Directory.EnumerateFiles(fullPath, "*.json");
        logger.LogDebug("Found {filesCount} files in {folderName} for report type {kubernetesObjectType}", files.Count(), fullPath, typeof(TTrivyReport).Name);

        await Parallel.ForEachAsync(files, ct, async (file, token) =>
        {
            bool isValidFile = false;
            try
            {
                logger.LogDebug("Processing file {fileName} for report type {kubernetesObjectType}", file, typeof(TTrivyReport).Name);
                
                await using var stream = File.OpenRead(file);

                // Try simple object
                try
                {
                    TTrivyReport? item = await JsonSerializer.DeserializeAsync<TTrivyReport>(stream, jsonOptions, token);
                    if (item?.Metadata != null)
                    {
                        item.Metadata.Uid = GuidUtils.GetDeterministicGuid($"{item.Metadata.Name}-{item.Metadata.NamespaceProperty}").ToString();
                        results.Add(item);
                        isValidFile = true;
                    }
                }
                catch(Exception ex)
                {
                    logger.LogError(ex, "Error deserializing file {fileName} as single object for report type {kubernetesObjectType}", file, typeof(TTrivyReport).Name);
                }

                // Reset stream for second attempt
                stream.Position = 0;

                // Try array
                try
                {
                    List<TTrivyReport>? items = await JsonSerializer.DeserializeAsync<List<TTrivyReport>>(stream, jsonOptions, token);
                    if (items != null)
                    {
                        foreach (TTrivyReport item in items)
                        {
                            if (item?.Metadata != null)
                            {
                                item.Metadata.Uid = GuidUtils.GetDeterministicGuid($"{item.Metadata.Name}-{item.Metadata.NamespaceProperty}").ToString();
                                results.Add(item);
                                isValidFile = true;
                            }
                        }
                        return;
                    }
                }
                catch
                {
                    // ignore, fallback to single object
                }


            }
            catch
            {
                // unreadable file - skip
            }
            
            if (!isValidFile)
                logger.LogWarning("Skipped invalid or unreadable file {fileName} for report type {kubernetesObjectType}", file, typeof(TTrivyReport).Name);
        });

        return [.. results];
    }

    public Task<IList<TTrivyReport>> GetAllReportsAsync(string key, CancellationToken? cancellationToken = null)
        => GetAllReportsAsync(cancellationToken);
}
