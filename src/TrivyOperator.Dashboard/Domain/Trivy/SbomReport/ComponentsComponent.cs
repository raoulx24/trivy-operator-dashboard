﻿using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Domain.Trivy.SbomReport;

public class ComponentsComponent
{
    [JsonPropertyName("bom-ref")]
    public string BomRef { get; set; } = string.Empty;
    [JsonPropertyName("licenses")]
    public LicenseContainer[]? Licenses { get; init; }
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("properties")]
    public Property[] Properties { get; init; } = [];

    [JsonPropertyName("purl")]
    public string Purl { get; init; } = string.Empty;

    [JsonPropertyName("supplier")]
    public Supplier? Supplier { get; init; }

    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; init; } = string.Empty;
}
