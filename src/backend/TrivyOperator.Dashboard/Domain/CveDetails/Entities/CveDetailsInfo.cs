using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.CveDetails.Entities;

// CVE details for one CVE, from the configured provider (CVE.org or NVD) - fetched on demand (CVE details dialog);
// not related to CVE Enrichment
// only what the dialog shows; the affected products (CVE.org), the configurations (NVD CPE match trees) and the
// cisa* fields (covered by KEV) are not kept
// Description - the english one
// Weaknesses - distinct CWE ids from all sources (NVD may add NVD-CWE-Other, NVD-CWE-noinfo)
public sealed record CveDetailsInfo(
    CveId Id,
    string Description,
    Timestamp? Published,
    Timestamp? LastModified,
    IReadOnlyList<CveDetailsCvss> Cvss,
    IReadOnlyList<string> Weaknesses,
    IReadOnlyList<CveDetailsReference> References);

// one CVSS score, as given by one source
// Source - the one that scored it: nvd@nist.gov, a CNA (e.g. security@apache.org, or a short name like "apache") or an
// ADP (e.g. CISA-ADP)
public sealed record CveDetailsCvss(
    string Version,
    string Source,
    decimal BaseScore,
    Severity Severity,
    string VectorString);

public sealed record CveDetailsReference(
    ResourceUrl Url,
    IReadOnlyList<string> Tags);
