using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.EndOfLife.Abstractions;
using TrivyOperator.Dashboard.Application.EndOfLife.Models;
using TrivyOperator.Dashboard.Application.EndOfLife.Options;
using TrivyOperator.Dashboard.Domain.EndOfLife.Entities;
using TrivyOperator.Dashboard.Domain.EndOfLife.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.EndOfLife.Mappers;
using TrivyOperator.Dashboard.Infrastructure.EndOfLife.Models;
using TrivyOperator.Dashboard.Infrastructure.EndOfLife.Serializations;
using TrivyOperator.Dashboard.Infrastructure.Shared.Utils;

namespace TrivyOperator.Dashboard.Infrastructure.EndOfLife.Services;

// failures are returned as results, not logged as errors - the caller decides how often to report them
public class EndOfLifeClient(
    HttpClient httpClient,
    IOptionsMonitor<EndOfLifeOptions> options,
    ILogger<EndOfLifeClient> logger) : IEndOfLifeProvider
{
    public async Task<EolProviderResult> GetProduct(
        string baseUrl,
        ProductId id,
        string? eTag,
        CancellationToken ctx = default)
    {
        string url = $"{baseUrl.TrimEnd('/')}/products/{Uri.EscapeDataString(id.Value)}";

        using CancellationTokenSource timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ctx);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, options.CurrentValue.RequestTimeoutInSeconds)));

        try
        {
            using HttpRequestMessage request = new(HttpMethod.Get, url);

            if (!string.IsNullOrEmpty(eTag) && EntityTagHeaderValue.TryParse(eTag, out EntityTagHeaderValue? entityTag))
            {
                request.Headers.IfNoneMatch.Add(entityTag);
            }

            using HttpResponseMessage response = await httpClient.SendAsync(request, timeoutCts.Token);

            switch (response.StatusCode)
            {
                case HttpStatusCode.NotModified:
                    return EolProviderResult.NotModified();
                case HttpStatusCode.NotFound:
                    return EolProviderResult.NotFound();
                case HttpStatusCode.TooManyRequests:
                    return EolProviderResult.Error($"Rate limited by {url}", response.GetRetryAfter());
            }

            if (!response.IsSuccessStatusCode)
            {
                return EolProviderResult.Error($"{url} returned {(int)response.StatusCode} {response.ReasonPhrase}");
            }

            await using Stream content = await response.Content.ReadAsStreamAsync(timeoutCts.Token);
            EndOfLifeProductResponse? productResponse = await JsonSerializer.DeserializeAsync(
                content,
                EndOfLifeJsonContext.Default.EndOfLifeProductResponse,
                timeoutCts.Token);
            EolProduct? product = productResponse?.ToDomain(id);

            return product is null
                ? EolProviderResult.Error($"{url} returned an empty or unexpected response")
                : EolProviderResult.Ok(product, response.Headers.ETag?.ToString());
        }
        catch (OperationCanceledException) when (ctx.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            return EolProviderResult.Error($"Timeout while calling {url}");
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException)
        {
            logger.LogDebug(ex, "Error fetching {url}", url);

            return EolProviderResult.Error($"Error calling {url} - {ex.Message}");
        }
    }
}
