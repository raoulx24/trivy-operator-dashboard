using TrivyOperator.Dashboard.Application.Nvd.Abstractions;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Vulnerabilities;

namespace TrivyOperator.Dashboard.Application.Nvd.Services;

// multi-context and file repository: the vulnerability reports are read on demand, so the CVEs are looked for in the
// slim reports of the current context (loaded first if not cached); no decoding is needed for that
// a report type that is not enabled has no provider (null)
public sealed class ReportEntriesNvdSubjects(
    IResourceEntryProvider<VulnerabilityReportSlim, Digest>? vulnerabilityReports,
    IResourceEntryProvider<ClusterVulnerabilityReportSlim, Uid>? clusterVulnerabilityReports
) : INvdSubjects
{
    public async Task<bool> Contains(CveId id, CancellationToken ctx = default)
    {
        if (!id.IsValid)
        {
            return false;
        }

        VulnerabilityId vulnerabilityId = new(id.Value);

        if (vulnerabilityReports is not null &&
            (await vulnerabilityReports.GetEntries(ctx)).Any(x => Contains(x.Vulnerabilities, vulnerabilityId)))
        {
            return true;
        }

        return clusterVulnerabilityReports is not null &&
               (await clusterVulnerabilityReports.GetEntries(ctx)).Any(x => Contains(x.Vulnerabilities, vulnerabilityId));
    }

    private static bool Contains(IReadOnlyList<VulnerabilitySlim> vulnerabilities, VulnerabilityId id)
    {
        foreach (VulnerabilitySlim vulnerability in vulnerabilities)
        {
            if (vulnerability.Id == id)
            {
                return true;
            }
        }

        return false;
    }
}
