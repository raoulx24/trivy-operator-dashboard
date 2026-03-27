using TrivyOperator.Dashboard.Domain.Trivy.VulnerabilityReport;

namespace TrivyOperator.Dashboard.Domain.VrHistory;

public class VrSummary
{
    public string NamespaceNameName { get; }
    public string Digest { get; }
    public long Critical { get; }
    public long High { get; }
    public long Medium { get; }
    public long Low { get; }
    public long Unknown { get; }
    public DateTime Timestamp { get; }

    public VrSummary(VulnerabilityReportCr vr)
    {
        ArgumentNullException.ThrowIfNull(vr);
        ArgumentNullException.ThrowIfNull(vr.Report);
        if (string.IsNullOrEmpty(vr.Metadata.NamespaceProperty)) throw new ArgumentException("Namespace cannot be null or empty.");
        if (string.IsNullOrEmpty(vr.ImageArtifact?.Digest)) throw new ArgumentException(nameof(vr.ImageArtifact));
        
        NamespaceNameName = vr.Metadata.NamespaceProperty;
        Digest = vr.ImageArtifact.Digest;
        Critical = vr.Report.Summary?.CriticalCount ?? 0;
        High = vr.Report.Summary?.HighCount ?? 0;
        Medium = vr.Report.Summary?.MediumCount ?? 0;
        Low = vr.Report.Summary?.LowCount ?? 0;
        Unknown = vr.Report.Summary?.UnknownCount ?? 0;
        Timestamp = vr.Report.UpdateTimestamp ?? vr.Metadata.CreationTimestamp ?? DateTime.UtcNow;
    }
}
