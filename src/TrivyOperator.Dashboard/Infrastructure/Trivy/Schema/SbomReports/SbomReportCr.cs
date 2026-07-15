using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Infrastructure.K8s.CustomResources;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.Abstracts;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Sboms;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Shared;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.SbomReports;

public class SbomReportCr : CustomResource, IHasArtifact
{
    [JsonPropertyName("report")]
    public ReportCr Report { get; init; } = new();
    
    public ArtifactCr Artifact => Report.Artifact;
}
