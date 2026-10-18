using TrivyOperator.Dashboard.Domain.EndOfLife.Entities;
using TrivyOperator.Dashboard.Domain.EndOfLife.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

namespace TrivyOperator.Dashboard.Application.EndOfLife.Models;

// internal cache entry (per product) of the hosted service - last outcome of fetching a product from endoflife.date
// Product is kept from a previous successful fetch when a later refetch fails
public sealed record EolProductFetch(
    ProductId Id,
    EolProviderStatus Status,
    EolProduct? Product,
    string? ETag,
    string? ErrorMessage,
    Timestamp FetchedAt,
    Timestamp NextFetchAt)
{
    public bool IsDue(Timestamp now) => now >= NextFetchAt;

    public EolProductFetch MarkStale() => this with { NextFetchAt = new Timestamp(DateTime.MinValue), };
}
