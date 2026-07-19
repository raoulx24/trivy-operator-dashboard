using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Text.Json;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Utils;
using TrivyOperator.Dashboard.Infrastructure.FileRepository.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.FileRepository.Options;
using TrivyOperator.Dashboard.Infrastructure.FileRepository.Services.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.K8s.CustomResources;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Abstract;

namespace TrivyOperator.Dashboard.Infrastructure.FileRepository.Services;

public class FileTrivyReportService<TKubernetesObject, TTrivyReport>(
    ITrivyReportMapper<TKubernetesObject, TTrivyReport> mapper,
    IFolderNameFactory folderNameFactory,
    IOptions<FileRepositoryOptions> options,
    ILogger<FileTrivyReportService<TKubernetesObject, TTrivyReport>> logger
) : IFileTrivyReportService<TTrivyReport>
    where TTrivyReport : class, ITrivyReport
    where TKubernetesObject : CustomResource
{
    public async Task<IReadOnlyDictionary<NamespaceName, IReadOnlyCollection<TTrivyReport>>> GetReportsByNamespaceAsync(
        CancellationToken ctx = default
    )
    {
        string folder = folderNameFactory.Get<TKubernetesObject>();
        string fullPath = Path.Combine(options.Value.BasePath, folder);

        if (!Directory.Exists(fullPath))
        {
            return new Dictionary<NamespaceName, IReadOnlyCollection<TTrivyReport>>();
        }

        JsonSerializerOptions jsonSerializerOptions = JsonUtils.GetKubernetesJsonSerializerOptions();
        JsonUtils.ConfigureJsonSerializerOptions(jsonSerializerOptions);

        string[] files = Directory.GetFiles(fullPath, "*.json");

        logger.LogDebug(
            "Found {filesCount} files in {folderName} for report type {reportType}",
            files.Length,
            fullPath,
            typeof(TTrivyReport).Name
        );

        ConcurrentDictionary<NamespaceName, ConcurrentBag<TTrivyReport>> results = new();

        await Parallel.ForEachAsync(
            files,
            new ParallelOptions
            {
                CancellationToken = ctx,
                MaxDegreeOfParallelism = Environment.ProcessorCount
            },
            async (file, token) =>
            {
                try
                {
                    await using FileStream stream = File.OpenRead(file);

                    char jsonRoot = await GetJsonRootCharacterAsync(stream, token);

                    stream.Position = 0;

                    if (jsonRoot == '{')
                    {
                        TKubernetesObject? resource =
                            await JsonSerializer.DeserializeAsync<TKubernetesObject>(
                                stream,
                                jsonSerializerOptions,
                                token
                            );

                        if (resource != null)
                        {
                            AddReport(resource);
                        }
                    }
                    else if (jsonRoot == '[')
                    {
                        List<TKubernetesObject>? resources =
                            await JsonSerializer.DeserializeAsync<List<TKubernetesObject>>(
                                stream,
                                jsonSerializerOptions,
                                token
                            );

                        if (resources != null)
                        {
                            foreach (TKubernetesObject resource in resources)
                            {
                                AddReport(resource);
                            }
                        }
                    }
                    else
                    {
                        logger.LogWarning(
                            "Skipped file {fileName}. Unexpected JSON root character {rootCharacter}",
                            file,
                            jsonRoot
                        );
                    }

                    void AddReport(TKubernetesObject resource)
                    {
                        NamespaceName namespaceName = new(resource.Metadata.NamespaceProperty);

                        TTrivyReport report = mapper.MapToDomain(resource, existing: null);

                        ConcurrentBag<TTrivyReport> reports = results.GetOrAdd(
                            namespaceName,
                            _ => new ConcurrentBag<TTrivyReport>()
                        );

                        reports.Add(report);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(
                        ex,
                        "Skipped invalid or unreadable file {fileName} for report type {reportType}",
                        file,
                        typeof(TTrivyReport).Name
                    );
                }
            }
        );

        return results.ToDictionary(pair => pair.Key, pair => (IReadOnlyCollection<TTrivyReport>)pair.Value.ToArray());
    }

    private static async Task<char> GetJsonRootCharacterAsync(Stream stream, CancellationToken cancellationToken)
    {
        byte[] buffer = new byte[256];

        int bytesRead = await stream.ReadAsync(buffer, cancellationToken);

        if (bytesRead == 0)
        {
            throw new InvalidDataException("JSON file is empty.");
        }

        int offset = 0;

        // Handle UTF-8 BOM if present
        if (bytesRead >= 3 && buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
        {
            offset = 3;
        }

        while (offset < bytesRead)
        {
            char c = (char)buffer[offset];

            if (!char.IsWhiteSpace(c))
            {
                return c;
            }

            offset++;
        }

        throw new InvalidDataException("JSON file contains only whitespace.");
    }
}
