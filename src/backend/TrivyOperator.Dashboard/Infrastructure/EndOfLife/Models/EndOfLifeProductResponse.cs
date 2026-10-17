using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Infrastructure.EndOfLife.Models;

// endoflife.date api v1 - GET /products/{product}
// see https://endoflife.date/docs/api/v1/
// fields that are "not provided" and fields that are null are both mapped to null
public class EndOfLifeProductResponse
{
    [JsonPropertyName("schema_version")]
    public string? SchemaVersion { get; init; }

    [JsonPropertyName("generated_at")]
    public DateTimeOffset? GeneratedAt { get; init; }

    [JsonPropertyName("last_modified")]
    public DateTimeOffset? LastModified { get; init; }

    [JsonPropertyName("result")]
    public EndOfLifeProductDetails? Result { get; init; }
}

public class EndOfLifeProductDetails
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("label")]
    public string? Label { get; init; }

    [JsonPropertyName("labels")]
    public EndOfLifeProductLabels? Labels { get; init; }

    [JsonPropertyName("links")]
    public EndOfLifeProductLinks? Links { get; init; }

    [JsonPropertyName("releases")]
    public EndOfLifeProductRelease[]? Releases { get; init; }
}

public class EndOfLifeProductLabels
{
    [JsonPropertyName("eol")]
    public string? Eol { get; init; }

    [JsonPropertyName("eoas")]
    public string? Eoas { get; init; }

    [JsonPropertyName("eoes")]
    public string? Eoes { get; init; }
}

public class EndOfLifeProductLinks
{
    [JsonPropertyName("html")]
    public string? Html { get; init; }

    [JsonPropertyName("releasePolicy")]
    public string? ReleasePolicy { get; init; }
}

public class EndOfLifeProductRelease
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("codename")]
    public string? Codename { get; init; }

    [JsonPropertyName("label")]
    public string? Label { get; init; }

    [JsonPropertyName("releaseDate")]
    public DateOnly? ReleaseDate { get; init; }

    [JsonPropertyName("isLts")]
    public bool IsLts { get; init; }

    [JsonPropertyName("ltsFrom")]
    public DateOnly? LtsFrom { get; init; }

    [JsonPropertyName("isEoas")]
    public bool? IsEoas { get; init; }

    [JsonPropertyName("eoasFrom")]
    public DateOnly? EoasFrom { get; init; }

    [JsonPropertyName("isEol")]
    public bool IsEol { get; init; }

    [JsonPropertyName("eolFrom")]
    public DateOnly? EolFrom { get; init; }

    [JsonPropertyName("isEoes")]
    public bool? IsEoes { get; init; }

    [JsonPropertyName("eoesFrom")]
    public DateOnly? EoesFrom { get; init; }

    [JsonPropertyName("isMaintained")]
    public bool IsMaintained { get; init; }

    [JsonPropertyName("latest")]
    public EndOfLifeProductVersion? Latest { get; init; }
}

public class EndOfLifeProductVersion
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("date")]
    public DateOnly? Date { get; init; }

    [JsonPropertyName("link")]
    public string? Link { get; init; }
}
