using TrivyOperator.Dashboard.Application.EndOfLife.Models;
using TrivyOperator.Dashboard.Domain.EndOfLife.ValueObjects;

namespace TrivyOperator.Dashboard.Application.EndOfLife.Abstractions;

public interface IEndOfLifeProvider
{
    // eTag - opaque validator from a previous Ok result; when provided, the provider may answer with NotModified
    Task<EolProviderResult> GetProduct(
        string baseUrl,
        ProductId id,
        string? eTag,
        CancellationToken ctx = default);
}
