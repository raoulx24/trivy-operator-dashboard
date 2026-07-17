using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Domain.TrivyOld.Report.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.K8s.CustomResources;

namespace TrivyOperator.Dashboard.Domain.TrivyOld.SbomReport;

public class OldSbomReportCr : CustomResource, ITrivyReportWithImage
{
    [JsonPropertyName("report")]
    public Report? Report { get; init; }

    public IArtifact? ImageArtifact => Report?.Artifact;
    public IRegistry? ImageRegistry => Report?.Registry;
    public DateTime? UpdateTimestamp => Report?.UpdateTimestamp ?? Metadata.CreationTimestamp;
}
