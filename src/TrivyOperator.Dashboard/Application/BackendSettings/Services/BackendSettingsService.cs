using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.BackendSettings.Models;
using TrivyOperator.Dashboard.Application.BackendSettings.Services.Abstractions;
using TrivyOperator.Dashboard.Application.History.VulnerabilityReportsHistory.Retention;
using TrivyOperator.Dashboard.Application.K8s.Services.Options;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory;
using TrivyOperator.Dashboard.Domain.TrivyOld.Services.FileRepository.Options;

namespace TrivyOperator.Dashboard.Application.BackendSettings.Services;

public class BackendSettingsService(
    IOptions<KubernetesOptions> k8sOptions, 
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
                        k8sOptions.Value.TrivyUseClusterComplianceReport,
                        frOptions.Value.BasePath,
                        frOptions.Value.ClusterComplianceReportCrSubpath
                    ),
                },
                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "ciar",
                    Name = "Cluster Infra Assessment Report",
                    Enabled = IsTrivyReportEnabled(
                        k8sOptions.Value.TrivyUseClusterInfraAssessmentReport,
                        frOptions.Value.BasePath,
                        frOptions.Value.ClusterInfraAssessmentReportCrSubpath
                    ),
                },
                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "crar",
                    Name = "Cluster RBAC Assessment Report",
                    Enabled = IsTrivyReportEnabled(
                        k8sOptions.Value.TrivyUseClusterRbacAssessmentReport,
                        frOptions.Value.BasePath,
                        frOptions.Value.ClusterRbacAssessmentReportCrSubpath
                    ),
                },
                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "csr",
                    Name = "Cluster SBOM Report",
                    Enabled = IsTrivyReportEnabled(
                        k8sOptions.Value.TrivyUseClusterSbomReport,
                        frOptions.Value.BasePath,
                        frOptions.Value.ClusterSbomReportCrSubpath
                    ),
                },
                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "cvr",
                    Name = "Cluster Vulnerability Report",
                    Enabled = IsTrivyReportEnabled(
                        k8sOptions.Value.TrivyUseClusterVulnerabilityReport,
                        frOptions.Value.BasePath,
                        frOptions.Value.ClusterVulnerabilityReportCrSubpath
                    ),
                },

                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "car",
                    Name = "Config Audit Report",
                    Enabled = IsTrivyReportEnabled(
                        k8sOptions.Value.TrivyUseConfigAuditReport,
                        frOptions.Value.BasePath,
                        frOptions.Value.ConfigAuditReportCrSubpath
                    ),
                },
                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "esr",
                    Name = "Exposed Secret Report",
                    Enabled = IsTrivyReportEnabled(
                        k8sOptions.Value.TrivyUseExposedSecretReport,
                        frOptions.Value.BasePath,
                        frOptions.Value.ExposedSecretReportCrSubpath
                    ),
                },
                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "iar",
                    Name = "Infra Assessment Report",
                    Enabled = IsTrivyReportEnabled(
                        k8sOptions.Value.TrivyUseInfraAssessmentReport,
                        frOptions.Value.BasePath,
                        frOptions.Value.InfraAssessmentReportCrSubpath
                    ),
                },
                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "rar",
                    Name = "RBAC Assessment Report",
                    Enabled = IsTrivyReportEnabled(
                        k8sOptions.Value.TrivyUseRbacAssessmentReport,
                        frOptions.Value.BasePath,
                        frOptions.Value.RbacAssessmentReportCrSubpath
                    ),
                },
                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "sr",
                    Name = "SBOM Report",
                    Enabled = IsTrivyReportEnabled(
                        k8sOptions.Value.TrivyUseSbomReport,
                        frOptions.Value.BasePath,
                        frOptions.Value.SbomReportCrSubpath
                    ),
                },
                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "vr",
                    Name = "Vulnerability Report",
                    Enabled = IsTrivyReportEnabled(
                        k8sOptions.Value.TrivyUseVulnerabilityReport,
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
            IsDefaultContextUsed = k8sOptions.Value.UseDefaultContext,
            IsKubeConfigUsed = !string.IsNullOrWhiteSpace(k8sOptions.Value.KubeConfigFileName),
            IsNamespaceListUsed = !string.IsNullOrWhiteSpace(k8sOptions.Value.NamespaceList),
            IsFileRepositoryUsed = !string.IsNullOrWhiteSpace(frOptions.Value.BasePath),
            VrHistoryMaxAgeDays = historyRetentionOptions.Value.KeepDays,
        };

        return Task.FromResult(backendSettingsDto);
    }


    private static bool IsTrivyReportEnabled(bool useTrivyReport, string pvcName, string subpath) => useTrivyReport &&
        (string.IsNullOrWhiteSpace(pvcName) ||
         (!string.IsNullOrWhiteSpace(pvcName) && !string.IsNullOrWhiteSpace(subpath)));
}
