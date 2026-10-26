using TrivyOperator.Dashboard.Application.Shared.Cache.Abstractions;
using TrivyOperator.Dashboard.Domain.CveEnrichment.Entities;
using TrivyOperator.Dashboard.Domain.CveEnrichment.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

namespace TrivyOperator.Dashboard.Application.CveEnrichment.Services;

// every source hosted service writes only its own part of a CveInfo; the parts of the other sources are preserved
// by retrying on concurrent changes
public static class CveInfoCacheExtensions
{
    // only the subjects decide which CVEs exist - a CVE that is not (or no longer) a subject is not written,
    // so a slow fetch does not bring back a swept CVE (a remaining race is cleaned up by the housekeeping)
    // returns true if the stored value changed
    public static bool UpdatePart(
        this ICache<CveId, CveInfo> infos,
        ICache<CveId, Timestamp> subjects,
        CveId id,
        Func<CveInfo, CveInfo> update)
    {
        while (subjects.ContainsKey(id))
        {
            if (!infos.TryGetValue(id, out CveInfo? current))
            {
                if (infos.TryAdd(id, update(CveInfo.Empty(id))))
                {
                    return true;
                }

                // added meanwhile
                continue;
            }

            CveInfo updated = update(current);

            if (updated == current)
            {
                return false;
            }

            if (infos.TryUpdate(id, updated, current))
            {
                return true;
            }

            // changed meanwhile
        }

        return false;
    }
}
