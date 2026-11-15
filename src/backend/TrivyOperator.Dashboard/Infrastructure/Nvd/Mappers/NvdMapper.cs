using TrivyOperator.Dashboard.Domain.CveDetails.Entities;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Infrastructure.Nvd.Models;
using TrivyOperator.Dashboard.Infrastructure.Shared.Utils;

namespace TrivyOperator.Dashboard.Infrastructure.Nvd.Mappers;

public static class NvdMapper
{
    private const string English = "en";
    private const string Primary = "Primary";

    // null when the response has no record with the requested id
    public static CveDetailsInfo? ToDomain(this NvdCveResponse response, CveId id)
    {
        NvdCve? cve = response.Vulnerabilities?
            .Select(static x => x.Cve)
            .FirstOrDefault(x => x is not null && new CveId(x.Id) == id);

        if (cve is null)
        {
            return null;
        }

        return new CveDetailsInfo(
            id,
            cve.Descriptions?
                .FirstOrDefault(static x => string.Equals(x.Lang, English, StringComparison.OrdinalIgnoreCase))?
                .Value?.Trim() ?? string.Empty,
            cve.Published is { } published ? new Timestamp(published) : null,
            cve.LastModified is { } lastModified ? new Timestamp(lastModified) : null,
            cve.Metrics.ToCvss(),
            [
                .. (cve.Weaknesses ?? [])
                    .SelectMany(static x => x.Description ?? [])
                    .Select(static x => x.Value?.Trim())
                    .OfType<string>()
                    .Where(static x => x.Length > 0)
                    .Distinct(StringComparer.OrdinalIgnoreCase),
            ],
            [
                .. (cve.References ?? [])
                    .Where(static x => !string.IsNullOrWhiteSpace(x.Url))
                    .Select(static x => new CveDetailsReference(new ResourceUrl(x.Url!.Trim()), x.Tags ?? [])),
            ]);
    }

    // newest CVSS version first; within a version, NVD's own (Primary) first
    private static IReadOnlyList<CveDetailsCvss> ToCvss(this NvdMetrics? metrics)
    {
        if (metrics is null)
        {
            return [];
        }

        return
        [
            .. new[] { metrics.CvssMetricV40, metrics.CvssMetricV31, metrics.CvssMetricV30, metrics.CvssMetricV2, }
                .SelectMany(static x => (x ?? []).OrderByDescending(static m => m.IsPrimary()))
                .Select(static x => x.ToDomain())
                .OfType<CveDetailsCvss>(),
        ];
    }

    private static CveDetailsCvss? ToDomain(this NvdCvssMetric metric)
    {
        if (metric.CvssData is not { BaseScore: { } baseScore, } data)
        {
            return null;
        }

        return new CveDetailsCvss(
            data.Version?.Trim() ?? string.Empty,
            metric.Source?.Trim() ?? string.Empty,
            baseScore,
            CvssSeverities.FromName(data.BaseSeverity ?? metric.BaseSeverity),
            data.VectorString?.Trim() ?? string.Empty);
    }

    private static bool IsPrimary(this NvdCvssMetric metric) =>
        string.Equals(metric.Type, Primary, StringComparison.OrdinalIgnoreCase);
}
