namespace TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

// what one source says about one CVE
// Data is set for Ok, and kept from a previous successful fetch when a later fetch fails
// FetchedAt is when the status was determined (for Error, the failed attempt)
public sealed record CveSourceResult<T>(
    CveSourceStatus Status,
    T? Data,
    string? ErrorMessage,
    Timestamp FetchedAt)
    where T : class
{
    public static CveSourceResult<T> Ok(T data, Timestamp fetchedAt) =>
        new(CveSourceStatuses.Ok, data, null, fetchedAt);

    public static CveSourceResult<T> NotListed(Timestamp fetchedAt) =>
        new(CveSourceStatuses.NotListed, null, null, fetchedAt);

    public static CveSourceResult<T> Failed(CveSourceResult<T>? previous, string errorMessage, Timestamp fetchedAt) =>
        new(CveSourceStatuses.Error, previous?.Data, errorMessage, fetchedAt);
}
