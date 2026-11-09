using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.BackendSettings.Queries.Models;
using TrivyOperator.Dashboard.Application.BackendSettings.Queries.Services.Abstractions;
using TrivyOperator.Dashboard.Application.CveEnrichment.Queries.Services.Abstractions;
using TrivyOperator.Dashboard.Application.EndOfLife.Queries.Services.Abstractions;
using TrivyOperator.Dashboard.Application.FileRepository.Options;
using TrivyOperator.Dashboard.Application.History.Queries.History.Services.Abstractions;
using TrivyOperator.Dashboard.Application.History.VulnerabilityReportsHistory.Retention;
using TrivyOperator.Dashboard.Application.Kubernetes.Options;
using TrivyOperator.Dashboard.Application.Nvd.Queries.Services.Abstractions;
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

namespace TrivyOperator.Dashboard.Application.BackendSettings.Queries.Services;

public class BackendSettingsService(
    IOptions<KubernetesOptions> kubernetesOptions,
    IOptions<FileRepositoryOptions> frOptions,
    IOptions<RetentionOptions> historyRetentionOptions,
    INvdService nvdService,
    ICveEnrichmentService cveEnrichmentService,
    IEndOfLifeService endOfLifeService,
    IVulnerabilityReportsHistoryService vrHistoryService,
    IClusterComplianceReportService clusterComplianceReportService,
    IClusterConfigAuditReportService clusterConfigAuditReportService,
    IClusterInfraAssessmentReportService clusterInfraAssessmentReportService,
    IClusterRbacAssessmentReportService clusterRbacAssessmentReportService,
    IClusterSbomReportService clusterSbomReportService,
    IClusterVulnerabilityReportService clusterVulnerabilityReportService,
    IConfigAuditReportService configAuditReportService,
    IExposedSecretReportService exposedSecretReportService,
    IInfraAssessmentReportService infraAssessmentReportService,
    IRbacAssessmentReportService rbacAssessmentReportService,
    ISbomReportService sbomReportService,
    IVulnerabilityReportService vulnerabilityReportService)
    : IBackendSettingsService
{
    public Task<BackendSettingsDto> GetBackendSettings()
    {
        BackendSettingsDto backendSettingsDto = new()
        {
            TrivyReportConfigDtos =
            [
                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "ccr",
                    Name = "Cluster Compliance Report",
                    Enabled = clusterComplianceReportService.IsEnabled,
                },
                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "ccar",
                    Name = "Cluster Config Audit Report",
                    Enabled = clusterConfigAuditReportService.IsEnabled,
                },
                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "ciar",
                    Name = "Cluster Infra Assessment Report",
                    Enabled = clusterInfraAssessmentReportService.IsEnabled,
                },
                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "crar",
                    Name = "Cluster RBAC Assessment Report",
                    Enabled = clusterRbacAssessmentReportService.IsEnabled,
                },
                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "csr",
                    Name = "Cluster SBOM Report",
                    Enabled = clusterSbomReportService.IsEnabled,
                },
                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "cvr",
                    Name = "Cluster Vulnerability Report",
                    Enabled = clusterVulnerabilityReportService.IsEnabled,
                },

                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "car",
                    Name = "Config Audit Report",
                    Enabled = configAuditReportService.IsEnabled,
                },
                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "esr",
                    Name = "Exposed Secret Report",
                    Enabled = exposedSecretReportService.IsEnabled,
                },
                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "iar",
                    Name = "Infra Assessment Report",
                    Enabled = infraAssessmentReportService.IsEnabled,
                },
                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "rar",
                    Name = "RBAC Assessment Report",
                    Enabled = rbacAssessmentReportService.IsEnabled,
                },
                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "sr",
                    Name = "SBOM Report",
                    Enabled = sbomReportService.IsEnabled,
                },
                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "vr",
                    Name = "Vulnerability Report",
                    Enabled = vulnerabilityReportService.IsEnabled,
                },
            ],
            IsDefaultContextUsed = kubernetesOptions.Value.UseDefaultContext,
            IsKubeConfigUsed = !string.IsNullOrWhiteSpace(kubernetesOptions.Value.KubeConfigFileName),
            IsNamespaceListUsed = !string.IsNullOrWhiteSpace(kubernetesOptions.Value.NamespaceList),
            IsFileRepositoryUsed = !string.IsNullOrWhiteSpace(frOptions.Value.BasePath),
            VrHistoryMaxAgeDays = historyRetentionOptions.Value.KeepDays,
            IsNvdEnabled = nvdService.IsEnabled,
            IsCveEnrichmentEnabled = cveEnrichmentService.IsEnabled,
            IsVrHistoryEnabled = vrHistoryService.IsEnabled,
            IsEolEnabled = endOfLifeService.IsEnabled,
        };

        return Task.FromResult(backendSettingsDto);
    }
}
