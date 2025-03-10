﻿using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Domain.Trivy.SbomReport;

public class Property
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; init; } = string.Empty;
}
