using System.Collections.Concurrent;
using TrivyOperator.Dashboard.Application.Shared.CveSubjects.Abstractions;
using TrivyOperator.Dashboard.Domain.Shared.Details;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Vulnerabilities;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.Details.Vulnerabilities;

// the CVE subjects read from the vulnerability definitions store, all contexts together
// - only the vulnerability ids that are CVEs are subjects (not GHSA, DLA and the like)
// - definitions is null when the vulnerability reports do not keep a definitions store (e.g. not in the default context);
//   there are no subjects then
public class InMemoryCveSubjects(
    IResourceDictionaryCache<VulnerabilityId, DetailEntry<VulnerabilityDefinition>>? definitions
) : ICveSubjects
{
    public bool Contains(CveId id) => TryGetLastSeen(id, out _);

    public bool TryGetLastSeen(CveId id, out Timestamp lastSeen)
    {
        lastSeen = default;

        if (!id.IsValid || definitions is null)
        {
            return false;
        }

        VulnerabilityId vulnerabilityId = new(id.Value);
        bool isFound = false;

        foreach (ConcurrentDictionary<VulnerabilityId, DetailEntry<VulnerabilityDefinition>> context in definitions.Values)
        {
            if (context.TryGetValue(vulnerabilityId, out DetailEntry<VulnerabilityDefinition>? entry) &&
                (!isFound || entry.LastSeenAt > lastSeen))
            {
                lastSeen = entry.LastSeenAt;
                isFound = true;
            }
        }

        return isFound;
    }

    public IReadOnlyDictionary<CveId, Timestamp> GetAll()
    {
        Dictionary<CveId, Timestamp> result = [];

        if (definitions is null)
        {
            return result;
        }

        foreach (ConcurrentDictionary<VulnerabilityId, DetailEntry<VulnerabilityDefinition>> context in definitions.Values)
        {
            foreach ((VulnerabilityId vulnerabilityId, DetailEntry<VulnerabilityDefinition> entry) in context)
            {
                CveId id = new(vulnerabilityId.Value);

                if (id.IsValid && (!result.TryGetValue(id, out Timestamp current) || entry.LastSeenAt > current))
                {
                    result[id] = entry.LastSeenAt;
                }
            }
        }

        return result;
    }
}
