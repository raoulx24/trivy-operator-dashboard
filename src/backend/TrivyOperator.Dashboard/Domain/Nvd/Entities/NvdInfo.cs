using TrivyOperator.Dashboard.Domain.Nvd.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.Nvd.Entities;

// NVD (CVE API 2.0) data for one CVE - fetched on demand (CVE details dialog); not related to CVE Enrichment
// only what the dialog shows; configurations (CPE match trees) and the cisa* fields (covered by KEV) are not kept
// Description - the english one
// Weaknesses - distinct CWE ids from all sources (may contain NVD-CWE-Other, NVD-CWE-noinfo)
public sealed record NvdInfo(
    CveId Id,
    NvdVulnStatus Status,
    string Description,
    Timestamp? Published,
    Timestamp? LastModified,
    IReadOnlyList<NvdCvss> Cvss,
    IReadOnlyList<string> Weaknesses,
    IReadOnlyList<NvdReference> References);

// one CVSS score, as given by one source
// Source - nvd@nist.gov, a CNA (e.g. security@apache.org) or an ADP id (a guid, e.g. CISA-ADP)
// IsPrimary - NVD's own score; the others are Secondary (CNA, ADP)
public sealed record NvdCvss(
    string Version,
    string Source,
    bool IsPrimary,
    decimal BaseScore,
    Severity Severity,
    string VectorString);

public sealed record NvdReference(
    ResourceUrl Url,
    IReadOnlyList<string> Tags);
