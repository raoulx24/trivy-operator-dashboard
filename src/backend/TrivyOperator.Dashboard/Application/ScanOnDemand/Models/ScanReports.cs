using TrivyOperator.Dashboard.Domain.ScanOnDemand.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Application.ScanOnDemand.Models;

// the reports found so far for one scan pod; ImageDigest is invalid while none has arrived
public sealed record ScanReports(
    Digest ImageDigest,
    VulnerabilityReport? VulnerabilityReport,
    ExposedSecretReport? ExposedSecretReport,
    SbomReport? SbomReport)
{
    public IReadOnlySet<ScanReportType> Present
    {
        get
        {
            HashSet<ScanReportType> present = [];

            if (VulnerabilityReport is not null)
            {
                present.Add(ScanReportType.VulnerabilityReport);
            }

            if (ExposedSecretReport is not null)
            {
                present.Add(ScanReportType.ExposedSecretReport);
            }

            if (SbomReport is not null)
            {
                present.Add(ScanReportType.SbomReport);
            }

            return present;
        }
    }
}
