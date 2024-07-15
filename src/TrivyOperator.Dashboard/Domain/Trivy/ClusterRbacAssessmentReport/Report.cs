﻿using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Domain.Trivy.VulnerabilityReport;

namespace TrivyOperator.Dashboard.Domain.Trivy.ClusterRbacAssessmentReport;

public class Report
{
    [JsonPropertyName("checks")]
    public Check[] Checks { get; set; } = [];

    [JsonPropertyName("scanner")]
    public Scanner? Scanner { get; set; }

    [JsonPropertyName("summary")]
    public Summary? Summary { get; set; }

    [JsonPropertyName("updateTimestamp")]
    public string? UpdateTimestamp { get; set; }
}
