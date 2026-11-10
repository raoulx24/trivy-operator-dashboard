using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.SecurityAssessments;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Application.Home.Services;

// the security assessment reports of all enabled types (config audit, RBAC, infra; namespaced and cluster scoped),
// flattened, with their (hydrated) failing checks
public sealed class HomeAssessmentReportsLoader(HomeReportSources sources)
{
    public async Task<List<HomeAssessmentReport>> Load(CancellationToken ctx = default)
    {
        List<HomeAssessmentReport> result = [];

        await Add(sources.ConfigAuditReports, "ConfigAudit", false, x => (x.Metadata, x.LastSeenAt, x.Checks));
        await Add(sources.ClusterConfigAuditReports, "ConfigAudit", true, x => (x.Metadata, x.LastSeenAt, x.Checks));
        await Add(sources.RbacAssessmentReports, "RbacAssessment", false, x => (x.Metadata, x.LastSeenAt, x.Checks));
        await Add(sources.ClusterRbacAssessmentReports, "RbacAssessment", true, x => (x.Metadata, x.LastSeenAt, x.Checks));
        await Add(sources.InfraAssessmentReports, "InfraAssessment", false, x => (x.Metadata, x.LastSeenAt, x.Checks));
        await Add(sources.ClusterInfraAssessmentReports, "InfraAssessment", true, x => (x.Metadata, x.LastSeenAt, x.Checks));

        return result;

        async Task Add<TReport>(
            IResourceProvider<TReport, Uid>? provider,
            string family,
            bool isClusterScoped,
            Func<TReport, (ReportMetadata Metadata, Timestamp LastSeenAt, IReadOnlyList<Check> Checks)> select)
        {
            if (provider is null)
            {
                return;
            }

            foreach (TReport report in await provider.GetResources(ctx))
            {
                (ReportMetadata metadata, Timestamp lastSeenAt, IReadOnlyList<Check> checks) = select(report);

                result.Add(new HomeAssessmentReport(
                    family,
                    typeof(TReport).Name,
                    metadata,
                    isClusterScoped,
                    lastSeenAt,
                    checks));
            }
        }
    }
}

public sealed record HomeAssessmentReport(
    // ConfigAudit, RbacAssessment, InfraAssessment
    string Family,
    // the report type, e.g. ClusterRbacAssessmentReport
    string Kind,
    ReportMetadata Metadata,
    bool IsClusterScoped,
    Timestamp LastSeenAt,
    IReadOnlyList<Check> Checks)
{
    public Uid Uid => Metadata.Uid;
    public NamespaceName NamespaceName => Metadata.NamespaceName;
}
