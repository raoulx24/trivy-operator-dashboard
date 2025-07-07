﻿using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Domain.Trivy.TrivyReport.Abstractions;

namespace TrivyOperator.Dashboard.Domain.Trivy.SbomReport;

public class Artifact : IArtifact
{
    [JsonPropertyName("digest")]
    public string Digest { get; init; } = string.Empty;
    [JsonPropertyName("repository")]
    public string Repository { get; init; } = string.Empty;

    [JsonPropertyName("tag")]
    public string Tag { get; init; } = string.Empty;
}
