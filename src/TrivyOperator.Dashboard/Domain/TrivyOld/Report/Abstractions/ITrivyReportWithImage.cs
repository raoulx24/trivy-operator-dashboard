using k8s.Models;

namespace TrivyOperator.Dashboard.Domain.TrivyOld.Report.Abstractions;

public interface ITrivyReportWithImage
{
    public V1ObjectMeta Metadata { get; }
    
    IArtifact? ImageArtifact { get; }
    IRegistry? ImageRegistry { get; }
    DateTime? UpdateTimestamp { get; }
}
