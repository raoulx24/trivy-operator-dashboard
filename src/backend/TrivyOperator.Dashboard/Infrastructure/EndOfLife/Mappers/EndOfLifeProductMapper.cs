using TrivyOperator.Dashboard.Domain.EndOfLife.Entities;
using TrivyOperator.Dashboard.Domain.EndOfLife.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.EndOfLife.Models;

namespace TrivyOperator.Dashboard.Infrastructure.EndOfLife.Mappers;

public static class EndOfLifeProductMapper
{
    private const string DefaultEolLabel = "End of Life";

    public static EolProduct? ToDomain(this EndOfLifeProductResponse response, ProductSlug requestedSlug)
    {
        EndOfLifeProductDetails? details = response.Result;

        if (details is null)
        {
            return null;
        }

        ProductSlug slug = new(details.Name);

        return new EolProduct(
            slug.IsValid ? slug : requestedSlug,
            details.Label ?? requestedSlug.Value,
            new ResourceUrl(details.Links?.Html),
            new ResourceUrl(details.Links?.ReleasePolicy),
            new EolPhaseLabels(
                details.Labels?.Eol ?? DefaultEolLabel,
                details.Labels?.Eoas,
                details.Labels?.Eoes),
            [.. (details.Releases ?? []).Select(static x => x.ToDomain()),],
            response.LastModified is { } lastModified ? new Timestamp(lastModified.UtcDateTime) : null);
    }

    private static EolRelease ToDomain(this EndOfLifeProductRelease release)
    {
        EolCycle cycle = new(release.Name);

        return new EolRelease(
            cycle,
            release.Codename,
            release.Label ?? cycle.Value,
            release.ReleaseDate,
            release.IsLts,
            release.LtsFrom,
            release.IsEol,
            release.EolFrom,
            release.IsMaintained,
            release.IsEoas,
            release.EoasFrom,
            release.IsEoes,
            release.EoesFrom,
            release.Latest is { Name: not null, } latest
                ? new EolLatestVersion(latest.Name, latest.Date, new ResourceUrl(latest.Link))
                : null);
    }
}
