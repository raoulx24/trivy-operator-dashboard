using System.Globalization;
using TrivyOperator.Dashboard.Domain.CveEnrichment.Entities;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.CveEnrichment.Models;

namespace TrivyOperator.Dashboard.Infrastructure.CveEnrichment.Mappers;

public static class EpssMapper
{
    // items without a valid CVE id, score, percentile or date are skipped
    public static IReadOnlyList<EpssScore> ToDomain(this EpssResponse response) =>
    [
        .. (response.Data ?? [])
            .Select(static x => x.ToDomain())
            .OfType<EpssScore>(),
    ];

    private static EpssScore? ToDomain(this EpssData data)
    {
        CveId id = new(data.Cve);

        if (!id.IsValid ||
            !TryParseProbability(data.Epss, out double score) ||
            !TryParseProbability(data.Percentile, out double percentile) ||
            data.Date is not { } date)
        {
            return null;
        }

        return new EpssScore(id, score, percentile, date);
    }

    private static bool TryParseProbability(string? value, out double result) =>
        double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result) &&
        result is >= 0 and <= 1;
}
