using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

namespace TrivyOperator.Dashboard.Application.Shared.CveSubjects.Abstractions;

// the distinct CVEs of the stored vulnerability reports, shared by CVE Enrichment and NVD
// - context agnostic: a CVE is a subject when a report of any Kubernetes context contains it
// - the value of a CVE is the newest scan timestamp of a report containing it
// - the lifetime is the one of the vulnerability definitions (kept while referenced by a stored report, plus a grace)
public interface ICveSubjects
{
    bool Contains(CveId id);

    bool TryGetLastSeen(CveId id, out Timestamp lastSeen);

    // a snapshot of all the subjects
    IReadOnlyDictionary<CveId, Timestamp> GetAll();
}
