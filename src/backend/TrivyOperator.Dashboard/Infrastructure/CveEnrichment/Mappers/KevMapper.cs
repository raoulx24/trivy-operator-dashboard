using TrivyOperator.Dashboard.Domain.CveEnrichment.Entities;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.CveEnrichment.Models;

namespace TrivyOperator.Dashboard.Infrastructure.CveEnrichment.Mappers;

public static class KevMapper
{
    private const string KnownRansomwareCampaignUse = "Known";

    // entries without a valid CVE id or without dates are skipped (all of them are required by the schema)
    public static IReadOnlyList<KevEntry> ToDomain(this KevCatalogResponse response) =>
    [
        .. (response.Vulnerabilities ?? [])
            .Select(static x => x.ToDomain())
            .OfType<KevEntry>(),
    ];

    private static KevEntry? ToDomain(this KevVulnerability vulnerability)
    {
        CveId id = new(vulnerability.CveId);

        if (!id.IsValid || vulnerability.DateAdded is not { } dateAdded || vulnerability.DueDate is not { } dueDate)
        {
            return null;
        }

        return new KevEntry(
            id,
            dateAdded,
            dueDate,
            string.Equals(
                vulnerability.KnownRansomwareCampaignUse?.Trim(),
                KnownRansomwareCampaignUse,
                StringComparison.OrdinalIgnoreCase)
                ? true
                : null,
            vulnerability.RequiredAction?.Trim() ?? string.Empty);
    }
}
