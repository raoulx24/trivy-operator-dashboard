using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.History.VulnerabilityReportsHistory.Retention;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.Options;
using TrivyOperator.Dashboard.Application.Queries.BackendSettings.Models;
using TrivyOperator.Dashboard.Application.Queries.BackendSettings.Options;
using TrivyOperator.Dashboard.Application.Queries.BackendSettings.Services.Abstractions;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory;
using TrivyOperator.Dashboard.Infrastructure.FileRepository.Options;

namespace TrivyOperator.Dashboard.Application.Queries.BackendSettings.Services;

public class BackendSettingsService(
    IOptions<KubernetesOptions> kubernetesOptions,
    IOptions<EnabledTrivyReportsOptions> enabledTrivyReportsOptions,
    IOptions<FileRepositoryOptions> frOptions,
    IOptions<RetentionOptions> historyRetentionOptions,
    IOptions<VulnerabilityReportsHistoryOptions> vrHistoryOptions)
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
                    Enabled = IsTrivyReportEnabled(
                        enabledTrivyReportsOptions.Value.ClusterComplianceReport,
                        frOptions.Value.BasePath,
                        frOptions.Value.ClusterComplianceReportCrSubpath
                    ),
                },
                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "ccar",
                    Name = "Cluster Config Audit Report",
                    Enabled = IsTrivyReportEnabled(
                        enabledTrivyReportsOptions.Value.ClusterConfigAuditReport,
                        frOptions.Value.BasePath,
                        frOptions.Value.ClusterConfigAuditReportCrSubpath
                    ),
                },
                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "ciar",
                    Name = "Cluster Infra Assessment Report",
                    Enabled = IsTrivyReportEnabled(
                        enabledTrivyReportsOptions.Value.ClusterInfraAssessmentReport,
                        frOptions.Value.BasePath,
                        frOptions.Value.ClusterInfraAssessmentReportCrSubpath
                    ),
                },
                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "crar",
                    Name = "Cluster RBAC Assessment Report",
                    Enabled = IsTrivyReportEnabled(
                        enabledTrivyReportsOptions.Value.ClusterRbacAssessmentReport,
                        frOptions.Value.BasePath,
                        frOptions.Value.ClusterRbacAssessmentReportCrSubpath
                    ),
                },
                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "csr",
                    Name = "Cluster SBOM Report",
                    Enabled = IsTrivyReportEnabled(
                        enabledTrivyReportsOptions.Value.ClusterSbomReport,
                        frOptions.Value.BasePath,
                        frOptions.Value.ClusterSbomReportCrSubpath
                    ),
                },
                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "cvr",
                    Name = "Cluster Vulnerability Report",
                    Enabled = IsTrivyReportEnabled(
                        enabledTrivyReportsOptions.Value.ClusterVulnerabilityReport,
                        frOptions.Value.BasePath,
                        frOptions.Value.ClusterVulnerabilityReportCrSubpath
                    ),
                },

                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "car",
                    Name = "Config Audit Report",
                    Enabled = IsTrivyReportEnabled(
                        enabledTrivyReportsOptions.Value.ConfigAuditReport,
                        frOptions.Value.BasePath,
                        frOptions.Value.ConfigAuditReportCrSubpath
                    ),
                },
                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "esr",
                    Name = "Exposed Secret Report",
                    Enabled = IsTrivyReportEnabled(
                        enabledTrivyReportsOptions.Value.ExposedSecretReport,
                        frOptions.Value.BasePath,
                        frOptions.Value.ExposedSecretReportCrSubpath
                    ),
                },
                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "iar",
                    Name = "Infra Assessment Report",
                    Enabled = IsTrivyReportEnabled(
                        enabledTrivyReportsOptions.Value.InfraAssessmentReport,
                        frOptions.Value.BasePath,
                        frOptions.Value.InfraAssessmentReportCrSubpath
                    ),
                },
                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "rar",
                    Name = "RBAC Assessment Report",
                    Enabled = IsTrivyReportEnabled(
                        enabledTrivyReportsOptions.Value.RbacAssessmentReport,
                        frOptions.Value.BasePath,
                        frOptions.Value.RbacAssessmentReportCrSubpath
                    ),
                },
                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "sr",
                    Name = "SBOM Report",
                    Enabled = IsTrivyReportEnabled(
                        enabledTrivyReportsOptions.Value.SbomReport,
                        frOptions.Value.BasePath,
                        frOptions.Value.SbomReportCrSubpath
                    ),
                },
                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "vr",
                    Name = "Vulnerability Report",
                    Enabled = IsTrivyReportEnabled(
                        enabledTrivyReportsOptions.Value.VulnerabilityReport,
                        frOptions.Value.BasePath,
                        frOptions.Value.VulnerabilityReportCrSubpath
                    ),
                },
                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "vrh",
                    Name = "Vulnerability Report History",
                    Enabled = vrHistoryOptions.Value.Enabled,
                },
            ],
            IsDefaultContextUsed = kubernetesOptions.Value.UseDefaultContext,
            IsKubeConfigUsed = !string.IsNullOrWhiteSpace(kubernetesOptions.Value.KubeConfigFileName),
            IsNamespaceListUsed = !string.IsNullOrWhiteSpace(kubernetesOptions.Value.NamespaceList),
            IsFileRepositoryUsed = !string.IsNullOrWhiteSpace(frOptions.Value.BasePath),
            VrHistoryMaxAgeDays = historyRetentionOptions.Value.KeepDays,
        };

        return Task.FromResult(backendSettingsDto);
    }


    private static bool IsTrivyReportEnabled(bool useTrivyReport, string pvcName, string subpath) => useTrivyReport &&
        (string.IsNullOrWhiteSpace(pvcName) ||
         (!string.IsNullOrWhiteSpace(pvcName) && !string.IsNullOrWhiteSpace(subpath)));
}
