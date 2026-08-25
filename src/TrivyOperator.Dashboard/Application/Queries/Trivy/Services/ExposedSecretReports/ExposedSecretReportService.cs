using TrivyOperator.Dashboard.Application.Queries.Trivy.Mappers;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ExposedSecretReports.Abstractions;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ExposedSecretReports;

public class ExposedSecretReportService(
    IResourceProvider<ExposedSecretReport, Digest> resourceProvider,
    ILogger<ExposedSecretReportService> logger)
    : IExposedSecretReportService
{
    public async Task<IEnumerable<ExposedSecretReportDto>> GetExposedSecretReportDtos(
        string? namespaceName = null,
        CancellationToken ctx = default)
    {
        IReadOnlyList<ExposedSecretReport> reports =
            await GetReports(resourceProvider, namespaceName, ctx);

        return reports.SelectMany(static x => x.ToDtos());
    }

    public async Task<ExposedSecretReportDto?> GetExposedSecretReportDtoByUid(
        string uid,
        CancellationToken ctx = default)
    {
        IReadOnlyList<ExposedSecretReport> values =
            await resourceProvider.GetResourceSummaries(ctx);

        Uid valueUid = new(uid);

        ExposedSecretReport? report = values
            .FirstOrDefault(x =>
                x.Occurrences.Any(y => y.Metadata.Uid == valueUid));

        return report?.ToDtos().FirstOrDefault(x => x.Uid == valueUid.Value);
    }

    public async Task<IEnumerable<ExposedSecretReportDenormalizedDto>>
        GetExposedSecretReportDenormalizedDtos(
            string? namespaceName = null,
            CancellationToken ctx = default)
    {
        IReadOnlyList<ExposedSecretReport> reports =
            await GetReports(resourceProvider, namespaceName, ctx);

        return reports.SelectMany(static x => x.ToDenormalizedDtos());
    }

    public async Task<IEnumerable<ExposedSecretReportImageDto>>
        GetExposedSecretReportImageDtos(
            string? namespaceName = null,
            IReadOnlySet<int>? includedSeverities = null,
            CancellationToken ctx = default)
    {
        IReadOnlyList<ExposedSecretReport> reports;

        if (includedSeverities is null)
        {
            reports = await resourceProvider.GetResources(ctx);
        }
        else
        {
            IReadOnlyList<ExposedSecretReport> summaries =
                await resourceProvider.GetResourceSummaries(ctx);

            IReadOnlyList<Digest> digests =
            [
                .. summaries
                    .Where(x => x.SeverityCounters.HasAnyOf(includedSeverities))
                    .Select(x => x.Id),
            ];

            reports = await resourceProvider.GetResources(digests, ctx);
        }

        return reports.Select(static x => x.ToDto());
    }

    private static async Task<IReadOnlyList<ExposedSecretReport>> GetReports(
        IResourceProvider<ExposedSecretReport, Digest> resourceProvider,
        string? namespaceName,
        CancellationToken ctx)
    {
        if (string.IsNullOrEmpty(namespaceName))
            return await resourceProvider.GetResources(ctx);

        NamespaceName namespaceNameValue = new(namespaceName);

        IReadOnlyList<ExposedSecretReport> summaries =
            await resourceProvider.GetResourceSummaries(ctx);

        IReadOnlyList<Digest> digests =
        [
            .. summaries
                .Where(x =>
                    x.Occurrences.Any(y =>
                        y.Metadata.NamespaceName == namespaceNameValue))
                .Select(x => x.Id),
        ];

        return await resourceProvider.GetResources(digests, ctx);
    }
}
