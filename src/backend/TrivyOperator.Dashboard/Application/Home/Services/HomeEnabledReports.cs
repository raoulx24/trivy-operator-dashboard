using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ClusterComplianceReports.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ClusterConfigAuditReports.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ClusterInfraAssessmentReports.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ClusterRbacAssessmentReports.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ClusterSbomReports.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ClusterVulnerabilityReports.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ConfigAuditReports.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ExposedSecretReports.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.InfraAssessmentReports.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.RbacAssessmentReports.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.SbomReports.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.VulnerabilityReports.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;

namespace TrivyOperator.Dashboard.Application.Home.Services;

// which report types are enabled, as the backend settings page tells it: the IsEnabled of the query service of each type
// (a type that is turned off in EnabledTrivyReports has a null service). Not the presence of a provider: a disabled type
// can still have an empty one (NullResourceRepository), and the slim and the full report types do not agree on it.
public sealed class HomeEnabledReports(
    IVulnerabilityReportService vulnerabilityReportService,
    IClusterVulnerabilityReportService clusterVulnerabilityReportService,
    IConfigAuditReportService configAuditReportService,
    IClusterConfigAuditReportService clusterConfigAuditReportService,
    IRbacAssessmentReportService rbacAssessmentReportService,
    IClusterRbacAssessmentReportService clusterRbacAssessmentReportService,
    IInfraAssessmentReportService infraAssessmentReportService,
    IClusterInfraAssessmentReportService clusterInfraAssessmentReportService,
    IExposedSecretReportService exposedSecretReportService,
    ISbomReportService sbomReportService,
    IClusterSbomReportService clusterSbomReportService,
    IClusterComplianceReportService clusterComplianceReportService)
{
    // the names of the report types (as in the Kind of a report), in the order the page lists them
    public IReadOnlyList<string> Kinds { get; } =
    [
        .. new (string Kind, bool IsEnabled)[]
        {
            (nameof(VulnerabilityReport), vulnerabilityReportService.IsEnabled),
            (nameof(ClusterVulnerabilityReport), clusterVulnerabilityReportService.IsEnabled),
            (nameof(ConfigAuditReport), configAuditReportService.IsEnabled),
            (nameof(ClusterConfigAuditReport), clusterConfigAuditReportService.IsEnabled),
            (nameof(RbacAssessmentReport), rbacAssessmentReportService.IsEnabled),
            (nameof(ClusterRbacAssessmentReport), clusterRbacAssessmentReportService.IsEnabled),
            (nameof(InfraAssessmentReport), infraAssessmentReportService.IsEnabled),
            (nameof(ClusterInfraAssessmentReport), clusterInfraAssessmentReportService.IsEnabled),
            (nameof(ExposedSecretReport), exposedSecretReportService.IsEnabled),
            (nameof(SbomReport), sbomReportService.IsEnabled),
            (nameof(ClusterSbomReport), clusterSbomReportService.IsEnabled),
            (nameof(ClusterComplianceReport), clusterComplianceReportService.IsEnabled),
        }.Where(x => x.IsEnabled).Select(x => x.Kind),
    ];
}
