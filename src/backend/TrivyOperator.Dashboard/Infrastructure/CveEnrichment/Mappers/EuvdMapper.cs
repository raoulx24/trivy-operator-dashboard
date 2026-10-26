using System.Globalization;
using TrivyOperator.Dashboard.Domain.CveEnrichment.Entities;
using TrivyOperator.Dashboard.Domain.CveEnrichment.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.CveEnrichment.Models;

namespace TrivyOperator.Dashboard.Infrastructure.CveEnrichment.Mappers;

public static class EuvdMapper
{
    // EUVD dates come without a time zone; they are taken as UTC
    private const string DateFormat = "MMM d, yyyy, h:mm:ss tt";

    // one entry per CVE alias; an item without CVE aliases is skipped
    public static IEnumerable<EuvdEntry> ToDomain(this EuvdItem item)
    {
        EuvdId euvdId = new(item.Id);
        Timestamp? exploitedSince = ParseTimestamp(item.ExploitedSince);
        double? epss = item.Epss is { } percent and >= 0 and <= 100 ? percent / 100 : null;

        foreach (string alias in (item.Aliases ?? string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            CveId id = new(alias);

            if (id.IsValid)
            {
                yield return new EuvdEntry(id, euvdId, exploitedSince, epss);
            }
        }
    }

    private static Timestamp? ParseTimestamp(string? value) =>
        DateTime.TryParseExact(
            value?.Trim(),
            DateFormat,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out DateTime result)
            ? new Timestamp(result)
            : null;
}
