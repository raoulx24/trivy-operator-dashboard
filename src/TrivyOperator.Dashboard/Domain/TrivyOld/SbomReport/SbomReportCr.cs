using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Domain.TrivyOld.CustomResources.Abstractions;
using TrivyOperator.Dashboard.Domain.TrivyOld.Report.Abstractions;

namespace TrivyOperator.Dashboard.Domain.TrivyOld.SbomReport;

public class SbomReportCr : CustomResource, ITrivyReportWithImage
{
    [JsonPropertyName("report")]
    public Report? Report { get; init; }

    public IArtifact? ImageArtifact => Report?.Artifact;
    public IRegistry? ImageRegistry => Report?.Registry;
    public DateTime? UpdateTimestamp => Report?.UpdateTimestamp ?? Metadata.CreationTimestamp;
}
