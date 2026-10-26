namespace TrivyOperator.Dashboard.Application.CveEnrichment.Models;

// Items are set only for Ok; for a per CVE source, requested CVEs missing from Items are not listed by the source
// Validator - opaque value (ETag, Last-Modified...) to pass back on the next call, so the provider can answer NotModified
public sealed record CveProviderResult<T>(
    CveProviderStatus Status,
    IReadOnlyList<T>? Items,
    string? Validator,
    string? ErrorMessage,
    TimeSpan? RetryAfter)
{
    public static CveProviderResult<T> Ok(IReadOnlyList<T> items, string? validator = null) =>
        new(CveProviderStatuses.Ok, items, validator, null, null);

    public static CveProviderResult<T> NotModified() =>
        new(CveProviderStatuses.NotModified, null, null, null, null);

    public static CveProviderResult<T> Error(string errorMessage, TimeSpan? retryAfter = null) =>
        new(CveProviderStatuses.Error, null, null, errorMessage, retryAfter);
}
