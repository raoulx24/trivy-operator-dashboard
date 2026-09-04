using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Threading.Channels;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Utils;
using TrivyOperator.Dashboard.Infrastructure.FileRepository.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.FileRepository.Options;
using TrivyOperator.Dashboard.Infrastructure.FileRepository.Services.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.K8s.CustomResources;
using TrivyOperator.Dashboard.Infrastructure.Persistence.Aggregators.Abstracts;

namespace TrivyOperator.Dashboard.Infrastructure.FileRepository.Services;

public class FileTrivyReportService<TKubernetesObject, TReport, TKey>(
    IFolderNameFactory folderNameFactory,
    IOptions<FileRepositoryOptions> options,
    IResourceAggregator<TKubernetesObject, TReport, TKey> aggregator,
    ILogger<FileTrivyReportService<TKubernetesObject, TReport, TKey>> logger)
    : IFileTrivyReportService<TReport, TKey>
    where TKubernetesObject : CustomResource
    where TReport : class, ITrivyReport<TKey>
    where TKey : notnull
{
    public async Task<IReadOnlyDictionary<TKey, TReport>> GetReportsAsync(
        CancellationToken ctx = default)
    {
        string folder = folderNameFactory.Get<TKubernetesObject>();
        string fullPath = Path.Combine(options.Value.BasePath, folder);

        if (!Directory.Exists(fullPath))
        {
            return new Dictionary<TKey, TReport>();
        }

        JsonSerializerOptions jsonOptions = JsonUtils.GetKubernetesJsonSerializerOptions();
        JsonUtils.ConfigureJsonSerializerOptions(jsonOptions);

        string[] files = Directory.GetFiles(fullPath, "*.json");

        logger.LogDebug(
            "Found {FilesCount} files in {Folder} for {ReportType}",
            files.Length,
            fullPath,
            typeof(TReport).Name);

        Channel<TKubernetesObject> channel =
            Channel.CreateBounded<TKubernetesObject>(
                new BoundedChannelOptions(Environment.ProcessorCount * 4)
                {
                    SingleReader = true,
                    SingleWriter = false,
                    FullMode = BoundedChannelFullMode.Wait
                });

        Task<IReadOnlyDictionary<TKey, TReport>> aggregation =
            aggregator.AggregateFromChannelAsync(channel.Reader, ctx);

        Task[] readers = files
            .Select(file => DeserializeFileAsync(
                file,
                channel.Writer,
                jsonOptions,
                ctx))
            .ToArray();

        try
        {
            await Task.WhenAll(readers);

            channel.Writer.Complete();

            return await aggregation;
        }
        catch (Exception ex)
        {
            channel.Writer.Complete(ex);

            throw;
        }
    }

    private async Task DeserializeFileAsync(
        string file,
        ChannelWriter<TKubernetesObject> writer,
        JsonSerializerOptions jsonOptions,
        CancellationToken cancellationToken)
    {
        try
        {
            await using FileStream stream = File.OpenRead(file);

            char jsonRoot = await GetJsonRootCharacterAsync(stream, cancellationToken);

            stream.Position = 0;

            switch (jsonRoot)
            {
                case '{':
                {
                    TKubernetesObject? resource =
                        await JsonSerializer.DeserializeAsync<TKubernetesObject>(
                            stream,
                            jsonOptions,
                            cancellationToken);

                    if (resource is not null)
                    {
                        await writer.WriteAsync(resource, cancellationToken);
                    }

                    break;
                }

                case '[':
                {
                    List<TKubernetesObject>? resources =
                        await JsonSerializer.DeserializeAsync<List<TKubernetesObject>>(
                            stream,
                            jsonOptions,
                            cancellationToken);

                    if (resources is not null)
                    {
                        foreach (TKubernetesObject resource in resources)
                        {
                            await writer.WriteAsync(resource, cancellationToken);
                        }
                    }

                    break;
                }

                default:
                    logger.LogWarning(
                        "Skipped file {File}. Unexpected JSON root character '{Root}'",
                        file,
                        jsonRoot);
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Skipped invalid or unreadable file {File} for report type {ReportType}",
                file,
                typeof(TReport).Name);
        }
    }

    private static async Task<char> GetJsonRootCharacterAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        byte[] buffer = new byte[256];

        int bytesRead = await stream.ReadAsync(buffer, cancellationToken);

        if (bytesRead == 0)
        {
            throw new InvalidDataException("JSON file is empty.");
        }

        int offset = 0;

        if (bytesRead >= 3 &&
            buffer[0] == 0xEF &&
            buffer[1] == 0xBB &&
            buffer[2] == 0xBF)
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
