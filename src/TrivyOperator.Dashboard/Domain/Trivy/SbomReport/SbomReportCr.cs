using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Domain.Trivy.CustomResources.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Report.Abstractions;

namespace TrivyOperator.Dashboard.Domain.Trivy.SbomReport;

public class SbomReportCr : CustomResource, ITrivyReportWithImage
{
    [JsonPropertyName("report")]
    public Report? Report { get; init; }

    public IArtifact? ImageArtifact => Report?.Artifact;
    public IRegistry? ImageRegistry => Report?.Registry;
}
