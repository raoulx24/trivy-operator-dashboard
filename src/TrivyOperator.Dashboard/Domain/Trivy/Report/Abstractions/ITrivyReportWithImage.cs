namespace TrivyOperator.Dashboard.Domain.Trivy.Report.Abstractions;

public interface ITrivyReportWithImage
{
    IArtifact? ImageArtifact { get; }
    IRegistry? ImageRegistry { get; }
}