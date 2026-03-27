using System.Security.Cryptography;
using System.Text;
using TrivyOperator.Dashboard.Domain.Trivy.VulnerabilityReport;

namespace TrivyOperator.Dashboard.Domain.VrHistory;

public class VrSnapshot
{
    public string NamespaceName { get; }
    public string Digest { get; }
    public string CvesHash { get; }
    public VulnerabilityReportCr Snapshot { get; }

    public VrSnapshot(VulnerabilityReportCr vr)
    {
        Snapshot = vr ?? throw new ArgumentNullException(nameof(vr));
        if (string.IsNullOrEmpty(vr.ImageArtifact?.Digest)) throw new ArgumentException(nameof(vr.ImageArtifact));

        NamespaceName = vr.Metadata.NamespaceProperty;
        Digest = vr.ImageArtifact.Digest;
        string cves = string.Join(
            '|',
            (vr.Report?.Vulnerabilities?.Select(x => x.VulnerabilityId) ?? [])
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
        );
        CvesHash = ComputeSha256(cves);
    }
    
    private static string ComputeSha256(string input)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(input);
        byte[] hash = SHA256.HashData(bytes);

        return Convert.ToHexString(hash); // uppercase hex
    }
}
