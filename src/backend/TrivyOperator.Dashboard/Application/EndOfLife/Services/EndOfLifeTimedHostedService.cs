using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.EndOfLife.Abstractions;
using TrivyOperator.Dashboard.Application.EndOfLife.Models;
using TrivyOperator.Dashboard.Application.EndOfLife.Options;
using TrivyOperator.Dashboard.Application.Shared.Cache.Abstractions;
using TrivyOperator.Dashboard.Domain.EndOfLife.Entities;
using TrivyOperator.Dashboard.Domain.EndOfLife.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

namespace TrivyOperator.Dashboard.Application.EndOfLife.Services;

// each tick (or on a rebuild request):
// - maps the collected subjects to endoflife.date products (mappings are read on every tick, so reloads apply)
// - fetches the products that are missing or past their TTL (all of them on rebuild) into the internal products cache
// - recomputes the public cache for all subjects
// a failed fetch is retried only after the error TTL, so errors are reported once per error TTL
public sealed class EndOfLifeTimedHostedService(
    IEndOfLifeProvider provider,
    IEndOfLifeRefreshTrigger trigger,
    IOptionsMonitor<EndOfLifeOptions> options,
    ICache<EolSubject, Timestamp> subjects,
    ICache<ProductId, EolProductFetch> products,
    ICache<EolSubject, EolInfo> eolInfos,
    ILogger<EndOfLifeTimedHostedService> logger
) : BackgroundService
{
    public override async Task StopAsync(CancellationToken ctx)
    {
        logger.LogInformation("End of Life Timed Hosted Service is stopping.");
        await base.StopAsync(ctx);
        logger.LogInformation("End of Life Timed Hosted Service stopped.");
    }

    protected override async Task ExecuteAsync(CancellationToken ctx)
    {
        logger.LogInformation("End of Life Timed Hosted Service started.");

        while (!ctx.IsCancellationRequested)
        {
            bool rebuild;

            try
            {
                TimeSpan interval = TimeSpan.FromSeconds(Math.Max(1, options.CurrentValue.ProcessingIntervalInSeconds));
                rebuild = await trigger.WaitAsync(interval, ctx);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            try
            {
                await ProcessAsync(rebuild, ctx);
            }
            catch (OperationCanceledException) when (ctx.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while processing end of life info - {exceptionMessage}", ex.Message);
            }
        }
    }

    private async Task ProcessAsync(bool rebuild, CancellationToken ctx)
    {
        EndOfLifeOptions currentOptions = options.CurrentValue;
        // the binder may replace the dictionary (and its comparer), so the lookup is rebuilt case insensitive
        Dictionary<string, string> mappings = new(currentOptions.ProductMappings, StringComparer.OrdinalIgnoreCase);

        if (rebuild)
        {
            logger.LogInformation("End of life rebuild requested - all products will be refetched.");

            foreach (ProductId productId in products.Keys.ToArray())
            {
                if (products.TryGetValue(productId, out EolProductFetch? fetch))
                {
                    products[productId] = fetch.MarkStale();
                }
            }
        }

        EolSubject[] currentSubjects = [.. subjects.Keys,];
        HashSet<ProductId> neededProductIds = [];

        foreach (EolSubject subject in currentSubjects)
        {
            if (TryGetProductId(subject, mappings, out ProductId productId))
            {
                neededProductIds.Add(productId);
            }
        }

        Timestamp now = Timestamp.Now();

        foreach (ProductId productId in neededProductIds)
        {
            products.TryGetValue(productId, out EolProductFetch? previous);

            if (previous is null || previous.IsDue(now))
            {
                products[productId] = await FetchProductAsync(productId, previous, currentOptions, ctx);
            }
        }

        foreach (EolSubject subject in currentSubjects)
        {
            eolInfos[subject] = Resolve(subject, mappings);
        }
    }

    private async Task<EolProductFetch> FetchProductAsync(
        ProductId id,
        EolProductFetch? previous,
        EndOfLifeOptions currentOptions,
        CancellationToken ctx)
    {
        EolProviderResult result = await provider.GetProduct(currentOptions.BaseUrl, id, previous?.ETag, ctx);
        Timestamp now = Timestamp.Now();

        switch (result.Status)
        {
            case EolProviderStatus.Ok when result.Product is not null:
                logger.LogDebug("End of life product {productId} fetched.", id);

                return new EolProductFetch(
                    id,
                    EolProviderStatus.Ok,
                    result.Product,
                    result.ETag,
                    null,
                    now,
                    AddMinutes(now, currentOptions.ProductTtlInMinutes));

            case EolProviderStatus.NotModified when previous?.Product is not null:
                return previous with
                {
                    Status = EolProviderStatus.Ok,
                    ErrorMessage = null,
                    FetchedAt = now,
                    NextFetchAt = AddMinutes(now, currentOptions.ProductTtlInMinutes),
                };

            case EolProviderStatus.NotFound:
                logger.LogWarning(
                    "End of life product {productId} was not found. Check the product mappings.",
                    id);

                return new EolProductFetch(
                    id,
                    EolProviderStatus.NotFound,
                    null,
                    null,
                    null,
                    now,
                    AddMinutes(now, currentOptions.NotFoundTtlInMinutes));

            default:
                // a NotModified without a cached product drops the ETag, so the next try is a full fetch
                string errorMessage = result.ErrorMessage ?? $"Unexpected {result.Status} response";
                Timestamp nextFetchAt = result.RetryAfter is { } retryAfter
                    ? new Timestamp(now.Value.Add(retryAfter))
                    : AddMinutes(now, currentOptions.ErrorTtlInMinutes);

                logger.LogWarning(
                    "Could not fetch end of life product {productId} - {errorMessage}. Next try at {nextFetchAt}. " +
                    "If endoflife.date is not reachable from the cluster, consider disabling EndOfLife.",
                    id,
                    errorMessage,
                    nextFetchAt);

                return new EolProductFetch(
                    id,
                    EolProviderStatus.Error,
                    previous?.Product,
                    previous?.Product is null ? null : previous.ETag,
                    errorMessage,
                    now,
                    nextFetchAt);
        }
    }

    private EolInfo Resolve(EolSubject subject, Dictionary<string, string> mappings)
    {
        if (!TryGetProductId(subject, mappings, out ProductId productId))
        {
            return EolInfo.NotApplicable(subject, EolNotApplicableReason.NoMapping);
        }

        if (!products.TryGetValue(productId, out EolProductFetch? fetch))
        {
            return EolInfo.Pending(subject);
        }

        // on a failed refetch the previously fetched product is still used
        if (fetch.Product is null)
        {
            return fetch.Status switch
            {
                EolProviderStatus.NotFound => EolInfo.NotApplicable(subject, EolNotApplicableReason.ProductNotFound),
                EolProviderStatus.Error => EolInfo.Failed(subject, fetch.ErrorMessage),
                _ => EolInfo.Pending(subject),
            };
        }

        EolRelease? release = fetch.Product.FindRelease(subject.Version);

        return release is null
            ? EolInfo.NotApplicable(subject, EolNotApplicableReason.CycleNotFound, fetch.Product)
            : EolInfo.Resolved(subject, fetch.Product, release);
    }

    // an empty mapping value disables a default mapping
    private static bool TryGetProductId(EolSubject subject, Dictionary<string, string> mappings, out ProductId id)
    {
        id = new ProductId();

        if (subject.Kind != EolSubjectKind.Os || !mappings.TryGetValue(subject.Name, out string? mappedProductId))
        {
            return false;
        }

        id = new ProductId(mappedProductId);

        return id.IsValid;
    }

    private static Timestamp AddMinutes(Timestamp timestamp, int minutes) =>
        new(timestamp.Value.AddMinutes(Math.Max(1, minutes)));
}
