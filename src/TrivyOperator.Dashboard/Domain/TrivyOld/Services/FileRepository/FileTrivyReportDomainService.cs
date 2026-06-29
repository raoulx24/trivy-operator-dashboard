using k8s;
using k8s.Models;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Text.Json;
using TrivyOperator.Dashboard.Domain.TrivyOld.Services.FileRepository.Abstractions;
using TrivyOperator.Dashboard.Domain.TrivyOld.Services.FileRepository.Options;
using TrivyOperator.Dashboard.Domain.Utils;

namespace TrivyOperator.Dashboard.Domain.TrivyOld.Services.FileRepository;

public class FileTrivyReportDomainService<TTrivyReport>(
    IFolderNameFactory folderNameFactory,
    IOptions<FileRepositoryOptions> options,
    ILogger<FileTrivyReportDomainService<TTrivyReport>> logger
) : IFileTrivyReportDomainService<TTrivyReport>
    where TTrivyReport : IKubernetesObject<V1ObjectMeta>, IMetadata<V1ObjectMeta>
{
    public async Task<IList<TTrivyReport>> GetAllReportsAsync(CancellationToken? cancellationToken = null)
    {
        CancellationToken ct = cancellationToken ?? CancellationToken.None;

        string folder = folderNameFactory.Get<TTrivyReport>();
        string fullPath = Path.Combine(options.Value.BasePath, folder);

        if (!Directory.Exists(fullPath))
        {
            return [];
        }

        JsonSerializerOptions jsonSerializerOptions = JsonUtils.GetKubernetesJsonSerializerOptions();
        JsonUtils.ConfigureJsonSerializerOptions(jsonSerializerOptions);

        ConcurrentBag<TTrivyReport> results = [];
        IEnumerable<string> files = Directory.EnumerateFiles(fullPath, "*.json");
        logger.LogDebug(
            "Found {filesCount} files in {folderName} for report type {kubernetesObjectType}",
            files.Count(),
            fullPath,
            typeof(TTrivyReport).Name
        );

        await Parallel.ForEachAsync(
            files,
            ct,
            async (file, token) =>
            {
                bool isValidFile = false;
                Exception? simpleObectException = null;
                Exception? arrayObjectException = null;
                Exception? fileException = null;


                try
                {
                    logger.LogDebug(
                        "Processing file {fileName} for report type {kubernetesObjectType}",
                        file,
                        typeof(TTrivyReport).Name
                    );

                    await using FileStream stream = File.OpenRead(file);

                    // Try a simple object
                    try
                    {
                        TTrivyReport? item = await JsonSerializer.DeserializeAsync<TTrivyReport>(
                            stream,
                            jsonSerializerOptions,
                            token
                        );
                        if (item?.Metadata != null)
                        {
                            item.Metadata.Uid = GuidUtils.GetDeterministicGuid(
                                    item.Metadata.Name, item.Metadata.NamespaceProperty, typeof(TTrivyReport).Name)
                                .ToString();
                            results.Add(item);
                            isValidFile = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogDebug(
                            ex,
                            "Error deserializing file {fileName} as single object for report type {kubernetesObjectType}",
                            file,
                            typeof(TTrivyReport).Name
                        );
                        simpleObectException = ex;
                    }

                    // Reset stream for the second attempt
                    stream.Position = 0;

                    // Try an array
                    try
                    {
                        List<TTrivyReport>? items =
                            await JsonSerializer.DeserializeAsync<List<TTrivyReport>>(
                                stream,
                                jsonSerializerOptions,
                                token
                            );
                        if (items != null)
                        {
                            foreach (TTrivyReport item in items)
                            {
                                if (item?.Metadata != null)
                                {
                                    item.Metadata.Uid = GuidUtils.GetDeterministicGuid(
                                            item.Metadata.Name, item.Metadata.NamespaceProperty, typeof(TTrivyReport).Name)
                                        .ToString();
                                    results.Add(item);
                                    isValidFile = true;
                                }
                            }

                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogDebug(
                            ex,
                            "Error deserializing file {fileName} as array for report type {kubernetesObjectType}",
                            file,
                            typeof(TTrivyReport).Name
                        );
                        arrayObjectException = ex;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogDebug(
                        ex,
                        "Error reading file {fileName} for report type {kubernetesObjectType}",
                        file,
                        typeof(TTrivyReport).Name
                    );
                    fileException = ex;
                }

                List<Exception> exceptions = [];
                if (simpleObectException != null)
                {
                    exceptions.Add(simpleObectException);
                }

                if (arrayObjectException != null)
                {
                    exceptions.Add(arrayObjectException);
                }

                if (fileException != null)
                {
                    exceptions.Add(fileException);
                }

                if (!isValidFile && exceptions.Count > 0)
                {
                    AggregateException aggregateException = new("Failed to process file", exceptions);
                    logger.LogWarning(
                        aggregateException,
                        "Skipped invalid or unreadable file {fileName} for report type {kubernetesObjectType}",
                        file,
                        typeof(TTrivyReport).Name
                    );
                }
                else if (!isValidFile)
                {
                    logger.LogWarning(
                        "Skipped invalid file {fileName} for report type {kubernetesObjectType}",
                        file,
                        typeof(TTrivyReport).Name
                    );
                }
            }
        );

        return [.. results,];
    }

    public async Task<IList<TTrivyReport>> GetAllReportsAsync(string key, CancellationToken? cancellationToken = null)
    {
        IList<TTrivyReport> resources = await GetAllReportsAsync(cancellationToken);
        return [.. resources.Where(r => r.Metadata.NamespaceProperty == key),];
    }
}
