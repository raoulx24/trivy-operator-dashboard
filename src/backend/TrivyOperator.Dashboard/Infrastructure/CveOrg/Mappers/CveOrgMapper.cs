using System.Text.RegularExpressions;
using TrivyOperator.Dashboard.Domain.CveDetails.Entities;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Infrastructure.CveOrg.Models;
using TrivyOperator.Dashboard.Infrastructure.Shared.Utils;

namespace TrivyOperator.Dashboard.Infrastructure.CveOrg.Mappers;

public static partial class CveOrgMapper
{
    private const string English = "en";
    private const string Published = "PUBLISHED";

    // null when the record is not a published one for the requested id (rejected ones are not shown)
    public static CveDetailsInfo? ToDomain(this CveOrgRecordResponse response, CveId id)
    {
        CveOrgContainers? containers = response.Containers;

        if (containers?.Cna is null ||
            response.CveMetadata is not { } metadata ||
            !string.Equals(metadata.State, Published, StringComparison.OrdinalIgnoreCase) ||
            new CveId(metadata.CveId) != id)
        {
            return null;
        }

        // the CNA first - the author of the record; then the ADPs in the order they are listed
        CveOrgContainer[] all = [containers.Cna, .. containers.Adp ?? [],];

        return new CveDetailsInfo(
            id,
            all.SelectMany(static x => x.Descriptions ?? []).ToDescription(),
            metadata.DatePublished is { } published ? new Timestamp(published) : null,
            metadata.DateUpdated is { } updated ? new Timestamp(updated) : null,
            all.ToCvss(),
            all.ToWeaknesses(),
            all.ToReferences());
    }

    // the english one (en, en-US...), else the first one
    private static string ToDescription(this IEnumerable<CveOrgLangString> descriptions)
    {
        CveOrgLangString[] all = [.. descriptions.Where(static x => !string.IsNullOrWhiteSpace(x.Value)),];

        return (all.FirstOrDefault(static x => x.Lang is not null &&
                                               x.Lang.StartsWith(English, StringComparison.OrdinalIgnoreCase)) ??
                all.FirstOrDefault())?.Value?.Trim() ?? string.Empty;
    }

    // newest CVSS version first; within a version, the container order (CNA, then ADPs)
    private static IReadOnlyList<CveDetailsCvss> ToCvss(this CveOrgContainer[] containers)
    {
        (string Source, CveOrgMetric Metric)[] metrics =
        [
            .. containers.SelectMany(static c => (c.Metrics ?? []).Select(m => (
                Source: c.ProviderMetadata?.ShortName?.Trim() ?? string.Empty,
                Metric: m))),
        ];

        return
        [
            .. Pick(metrics, static m => m.CvssV40, isV2: false)
                .Concat(Pick(metrics, static m => m.CvssV31, isV2: false))
                .Concat(Pick(metrics, static m => m.CvssV30, isV2: false))
                .Concat(Pick(metrics, static m => m.CvssV20, isV2: true)),
        ];
    }

    private static IEnumerable<CveDetailsCvss> Pick(
        (string Source, CveOrgMetric Metric)[] metrics,
        Func<CveOrgMetric, CveOrgCvss?> select,
        bool isV2)
    {
        foreach ((string source, CveOrgMetric metric) in metrics)
        {
            if (select(metric) is not { BaseScore: { } baseScore, } cvss)
            {
                continue;
            }

            Severity severity = string.IsNullOrWhiteSpace(cvss.BaseSeverity)
                ? CvssSeverities.FromScore(baseScore, isV2)
                : CvssSeverities.FromName(cvss.BaseSeverity);

            yield return new CveDetailsCvss(
                cvss.Version?.Trim() ?? string.Empty,
                source,
                baseScore,
                severity,
                cvss.VectorString?.Trim() ?? string.Empty);
        }
    }

    // distinct CWE ids; the id is taken from the text when cweId is not sent ("CWE-79 Cross-site Scripting")
    private static IReadOnlyList<string> ToWeaknesses(this CveOrgContainer[] containers) =>
    [
        .. containers
            .SelectMany(static c => c.ProblemTypes ?? [])
            .SelectMany(static p => p.Descriptions ?? [])
            .Select(static d => d.CweId?.Trim() is { Length: > 0, } cweId
                ? cweId
                : CweInText().Match(d.Description ?? string.Empty) is { Success: true, } match
                    ? match.Value.ToUpperInvariant()
                    : null)
            .OfType<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase),
    ];

    // the same url may be listed by the CNA and an ADP - one entry, with the tags of both
    private static IReadOnlyList<CveDetailsReference> ToReferences(this CveOrgContainer[] containers) =>
    [
        .. containers
            .SelectMany(static c => c.References ?? [])
            .Where(static x => !string.IsNullOrWhiteSpace(x.Url))
            .GroupBy(static x => x.Url!.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(static g => new CveDetailsReference(
                new ResourceUrl(g.Key),
                [.. g.SelectMany(static x => x.Tags ?? []).Distinct(StringComparer.OrdinalIgnoreCase),])),
    ];

    [GeneratedRegex(@"^CWE-\d+", RegexOptions.IgnoreCase)]
    private static partial Regex CweInText();
}
