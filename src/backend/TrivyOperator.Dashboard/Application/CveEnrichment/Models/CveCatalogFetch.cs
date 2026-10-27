using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

namespace TrivyOperator.Dashboard.Application.CveEnrichment.Models;

// internal state of a bulk source hosted service (KEV, EUVD) - last outcome of downloading the whole catalog
// Entries are kept from a previous successful download when a later one fails
public sealed record CveCatalogFetch<T>(
    CveSourceStatus Status,
    IReadOnlyDictionary<CveId, T>? Entries,
    string? Validator,
    string? ErrorMessage,
    Timestamp FetchedAt,
    Timestamp NextFetchAt)
    where T : class
{
    public bool IsDue(Timestamp now) => now >= NextFetchAt;

    public CveCatalogFetch<T> MarkStale() => this with { NextFetchAt = new Timestamp(DateTime.MinValue), };
}
