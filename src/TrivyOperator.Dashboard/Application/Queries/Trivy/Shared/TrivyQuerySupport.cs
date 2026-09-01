using TrivyOperator.Dashboard.Application.Queries.Shared;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Shared;

public static class TrivyQuerySupport
{
    public static async Task<IReadOnlyList<TResource>> GetResources<TResource, TId>(
        IResourceProvider<TResource, TId> resourceProvider,
        string? namespaceName,
        CancellationToken ctx = default)
        where TResource : ITrivyReport<TId>
    {
        IReadOnlyList<TResource> summaries =
            await resourceProvider.GetResourceSummaries(ctx);

        NamespaceName ns = new(namespaceName);

        // filter summaries first to avoid fetching reports we don't need
        IReadOnlyList<TId> ids =
        [
            .. summaries
                .Where(x => namespaceName is null || x.HasNamespaceName(ns))
                .Select(x => x.Id),
        ];

        return await resourceProvider.GetResources(ids, ctx);
    }
    
    public static async Task<QueryResponse<IReadOnlyList<TResource>>>
        GetResources<TResource, TId>(
            IResourceProvider<TResource, TId> resourceProvider,
            string? namespaceName,
            string? excludedSeverities,
            CancellationToken ctx = default)
        where TResource : IHasSeverityCounters, ITrivyReport<TId>
    {
        IReadOnlySet<int>? includedSeverityIds = null;

        if (excludedSeverities is not null)
        {
            includedSeverityIds = GetSeverityIdsToInclude(excludedSeverities);

            if (includedSeverityIds is null)
            {
                return new QueryResponse<IReadOnlyList<TResource>>(
                    [],
                    "At least one Severity must be selected");
            }

            // Aal severities included = no severity filter needed
            if (includedSeverityIds.Count == GetAllSeverityIds().Count)
            {
                includedSeverityIds = null;
            }
        }

        IReadOnlyList<TResource> reports;

        // if no filters, it means we can fetch the reports directly
        if (includedSeverityIds is null && namespaceName is null)
        {
            reports = await resourceProvider.GetResources(ctx);
        }
        else
        {
            IReadOnlyList<TResource> summaries =
                await resourceProvider.GetResourceSummaries(ctx);

            NamespaceName ns = new(namespaceName);

            // filter summaries first to avoid fetching reports we don't need
            IReadOnlyList<TId> ids =
            [
                .. summaries
                    .Where(x =>
                        (namespaceName is null || x.HasNamespaceName(ns)) &&
                        (includedSeverityIds is null ||
                         x.SeverityCounters.HasAnyOf(includedSeverityIds)))
                    .Select(x => x.Id),
            ];

            reports = await resourceProvider.GetResources(ids, ctx);
        }

        return new QueryResponse<IReadOnlyList<TResource>>(reports, null);
    }
    
    public static async Task<TResource?> GetImageDigestReportDtoByUid<TResource>(
        IResourceProvider<TResource, Digest> resourceProvider,
        string uid,
        CancellationToken ctx = default)
    where TResource : class, IImageReport
    {
        IReadOnlyList<TResource> values =
            await resourceProvider.GetResourceSummaries(ctx);

        Uid valueUid = new(uid);

        Digest? digest = values
            .FirstOrDefault(x =>
                x.Occurrences.Any(y => y.Metadata.Uid == valueUid))?
            .ImageDigest;

        if (digest is not { } d)
            return null;

        return await resourceProvider.GetResource(d, ctx); 
    }

    private static IReadOnlySet<int>? GetSeverityIdsToInclude(string? excludedSeverities)
    {
        HashSet<int> knownSeverityIds = [.. GetAllSeverityIds(),];

        if (string.IsNullOrWhiteSpace(excludedSeverities))
        {
            return knownSeverityIds;
        }

        foreach (string value in excludedSeverities.Split(','))
        {
            if (!int.TryParse(value, out int severityId) ||
                !knownSeverityIds.Contains(severityId))
            {
                return null;
            }

            knownSeverityIds.Remove(severityId);
        }

        return knownSeverityIds;
    }

    private static IReadOnlySet<int> GetAllSeverityIds() =>
        Severity.RankedSeverities.Select(static x => x.Rank).ToHashSet();
}
