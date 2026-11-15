using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Infrastructure.CveOrg.Models;

// CVE.org CVE Services API - GET /api/cve/{cveId} - a CVE Record (CVE JSON 5)
// see https://cveawg.mitre.org/api-docs/
// only the fields used by the dialog are mapped; the affected products, credits, timeline... are skipped by the reader
// dates carry a Z (UTC), or have no time zone (they are UTC then)
// the data is in containers: the CNA (the assigner, the record's author) and the ADPs (additional data providers, e.g.
// CISA-ADP, with the CISA Vulnrichment scores)
public class CveOrgRecordResponse
{
    [JsonPropertyName("cveMetadata")]
    public CveOrgMetadata? CveMetadata { get; init; }

    [JsonPropertyName("containers")]
    public CveOrgContainers? Containers { get; init; }
}

public class CveOrgMetadata
{
    [JsonPropertyName("cveId")]
    public string? CveId { get; init; }

    // PUBLISHED or REJECTED (seen); anything else is not shown
    [JsonPropertyName("state")]
    public string? State { get; init; }

    [JsonPropertyName("datePublished")]
    public DateTime? DatePublished { get; init; }

    [JsonPropertyName("dateUpdated")]
    public DateTime? DateUpdated { get; init; }
}

public class CveOrgContainers
{
    [JsonPropertyName("cna")]
    public CveOrgContainer? Cna { get; init; }

    [JsonPropertyName("adp")]
    public CveOrgContainer[]? Adp { get; init; }
}

public class CveOrgContainer
{
    [JsonPropertyName("providerMetadata")]
    public CveOrgProviderMetadata? ProviderMetadata { get; init; }

    [JsonPropertyName("descriptions")]
    public CveOrgLangString[]? Descriptions { get; init; }

    [JsonPropertyName("metrics")]
    public CveOrgMetric[]? Metrics { get; init; }

    [JsonPropertyName("problemTypes")]
    public CveOrgProblemType[]? ProblemTypes { get; init; }

    [JsonPropertyName("references")]
    public CveOrgReference[]? References { get; init; }
}

public class CveOrgProviderMetadata
{
    [JsonPropertyName("shortName")]
    public string? ShortName { get; init; }
}

public class CveOrgLangString
{
    [JsonPropertyName("lang")]
    public string? Lang { get; init; }

    [JsonPropertyName("value")]
    public string? Value { get; init; }
}

// one metric holds one CVSS version (the others are null) - or another format (e.g. ssvc), skipped
public class CveOrgMetric
{
    [JsonPropertyName("cvssV4_0")]
    public CveOrgCvss? CvssV40 { get; init; }

    [JsonPropertyName("cvssV3_1")]
    public CveOrgCvss? CvssV31 { get; init; }

    [JsonPropertyName("cvssV3_0")]
    public CveOrgCvss? CvssV30 { get; init; }

    [JsonPropertyName("cvssV2_0")]
    public CveOrgCvss? CvssV20 { get; init; }
}

public class CveOrgCvss
{
    [JsonPropertyName("version")]
    public string? Version { get; init; }

    [JsonPropertyName("vectorString")]
    public string? VectorString { get; init; }

    [JsonPropertyName("baseScore")]
    public decimal? BaseScore { get; init; }

    // not sent for v2
    [JsonPropertyName("baseSeverity")]
    public string? BaseSeverity { get; init; }
}

public class CveOrgProblemType
{
    [JsonPropertyName("descriptions")]
    public CveOrgProblemDescription[]? Descriptions { get; init; }
}

public class CveOrgProblemDescription
{
    // not always sent - then only the text, which may start with the id ("CWE-79 Cross-site Scripting")
    [JsonPropertyName("cweId")]
    public string? CweId { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }
}

public class CveOrgReference
{
    [JsonPropertyName("url")]
    public string? Url { get; init; }

    [JsonPropertyName("tags")]
    public string[]? Tags { get; init; }
}
