using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Infrastructure.Nvd.Models;

// NVD CVE API 2.0 - /rest/json/cves/2.0?cveId=
// see https://nvd.nist.gov/developers/vulnerabilities
// only the fields used by the dialog are mapped; configurations (large CPE trees) are skipped by the reader
// dates have no time zone, they are UTC
public class NvdCveResponse
{
    [JsonPropertyName("totalResults")]
    public int? TotalResults { get; init; }

    [JsonPropertyName("vulnerabilities")]
    public NvdVulnerabilityItem[]? Vulnerabilities { get; init; }
}

public class NvdVulnerabilityItem
{
    [JsonPropertyName("cve")]
    public NvdCve? Cve { get; init; }
}

public class NvdCve
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("vulnStatus")]
    public string? VulnStatus { get; init; }

    [JsonPropertyName("published")]
    public DateTime? Published { get; init; }

    [JsonPropertyName("lastModified")]
    public DateTime? LastModified { get; init; }

    [JsonPropertyName("descriptions")]
    public NvdLangString[]? Descriptions { get; init; }

    [JsonPropertyName("metrics")]
    public NvdMetrics? Metrics { get; init; }

    [JsonPropertyName("weaknesses")]
    public NvdWeakness[]? Weaknesses { get; init; }

    [JsonPropertyName("references")]
    public NvdReferenceItem[]? References { get; init; }
}

public class NvdLangString
{
    [JsonPropertyName("lang")]
    public string? Lang { get; init; }

    [JsonPropertyName("value")]
    public string? Value { get; init; }
}

public class NvdMetrics
{
    [JsonPropertyName("cvssMetricV40")]
    public NvdCvssMetric[]? CvssMetricV40 { get; init; }

    [JsonPropertyName("cvssMetricV31")]
    public NvdCvssMetric[]? CvssMetricV31 { get; init; }

    [JsonPropertyName("cvssMetricV30")]
    public NvdCvssMetric[]? CvssMetricV30 { get; init; }

    [JsonPropertyName("cvssMetricV2")]
    public NvdCvssMetric[]? CvssMetricV2 { get; init; }
}

public class NvdCvssMetric
{
    [JsonPropertyName("source")]
    public string? Source { get; init; }

    // Primary (NVD) or Secondary (CNA, ADP)
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("cvssData")]
    public NvdCvssData? CvssData { get; init; }

    // v2 only - for v3.x and v4.0 it is in cvssData
    [JsonPropertyName("baseSeverity")]
    public string? BaseSeverity { get; init; }
}

public class NvdCvssData
{
    [JsonPropertyName("version")]
    public string? Version { get; init; }

    [JsonPropertyName("vectorString")]
    public string? VectorString { get; init; }

    [JsonPropertyName("baseScore")]
    public decimal? BaseScore { get; init; }

    [JsonPropertyName("baseSeverity")]
    public string? BaseSeverity { get; init; }
}

public class NvdWeakness
{
    [JsonPropertyName("description")]
    public NvdLangString[]? Description { get; init; }
}

public class NvdReferenceItem
{
    [JsonPropertyName("url")]
    public string? Url { get; init; }

    [JsonPropertyName("tags")]
    public string[]? Tags { get; init; }
}
