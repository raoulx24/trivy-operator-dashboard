using TrivyOperator.Dashboard.Domain.EndOfLife.Entities;

namespace TrivyOperator.Dashboard.Application.EndOfLife.Models;

public enum EolProviderStatus
{
    Ok,
    NotModified,
    NotFound,
    Error,
}

public sealed record EolProviderResult(
    EolProviderStatus Status,
    EolProduct? Product,
    string? ETag,
    string? ErrorMessage,
    TimeSpan? RetryAfter)
{
    public static EolProviderResult Ok(EolProduct product, string? eTag) =>
        new(EolProviderStatus.Ok, product, eTag, null, null);

    public static EolProviderResult NotModified() => new(EolProviderStatus.NotModified, null, null, null, null);

    public static EolProviderResult NotFound() => new(EolProviderStatus.NotFound, null, null, null, null);

    public static EolProviderResult Error(string errorMessage, TimeSpan? retryAfter = null) =>
        new(EolProviderStatus.Error, null, null, errorMessage, retryAfter);
}
