using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.BackendSettings.Models;
using TrivyOperator.Dashboard.Application.BackendSettings.Services.Abstractions;
using TrivyOperator.Dashboard.Application.K8s.Services.Options;
using TrivyOperator.Dashboard.Domain.Trivy.Services.FileRepository.Options;

namespace TrivyOperator.Dashboard.Application.BackendSettings.Services;

public class BackendSettingsService(IOptions<KubernetesOptions> k8sOptions, IOptions<FileRepositoryOptions> frOptions) : IBackendSettingsService
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
                        frOptions.Value.PvcName,
                        frOptions.Value.ClusterComplianceReportCrSubpath),
                },
                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "ciar",
                    Name = "Cluster Infra Assessment Report",
                    Enabled = IsTrivyReportEnabled(
                        k8sOptions.Value.TrivyUseClusterInfraAssessmentReport,
                        frOptions.Value.PvcName,
                        frOptions.Value.ClusterInfraAssessmentReportCrSubpath),
                },
                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "crar",
                    Name = "Cluster RBAC Assessment Report",
                    Enabled = IsTrivyReportEnabled(
                        k8sOptions.Value.TrivyUseClusterRbacAssessmentReport,
                        frOptions.Value.PvcName,
                        frOptions.Value.ClusterRbacAssessmentReportCrSubpath),
                },
                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "csr",
                    Name = "Cluster SBOM Report",
                    Enabled = IsTrivyReportEnabled(
                        k8sOptions.Value.TrivyUseClusterSbomReport,
                        frOptions.Value.PvcName,
                        frOptions.Value.ClusterSbomReportCrSubpath),
                },
                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "cvr",
                    Name = "Cluster Vulnerability Report",
                    Enabled = IsTrivyReportEnabled(
                        k8sOptions.Value.TrivyUseClusterVulnerabilityReport,
                        frOptions.Value.PvcName,
                        frOptions.Value.ClusterVulnerabilityReportCrSubpath),
                },

                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "car",
                    Name = "Config Audit Report",
                    Enabled = IsTrivyReportEnabled(
                        k8sOptions.Value.TrivyUseConfigAuditReport,
                        frOptions.Value.PvcName,
                        frOptions.Value.ConfigAuditReportCrSubpath),
                },
                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "esr",
                    Name = "Exposed Secret Report",
                    Enabled = IsTrivyReportEnabled(
                        k8sOptions.Value.TrivyUseExposedSecretReport,
                        frOptions.Value.PvcName,
                        frOptions.Value.ExposedSecretReportCrSubpath),
                },
                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "iar",
                    Name = "Infra Assessment Report",
                    Enabled = IsTrivyReportEnabled(
                        k8sOptions.Value.TrivyUseInfraAssessmentReport,
                        frOptions.Value.PvcName,
                        frOptions.Value.InfraAssessmentReportCrSubpath),
                },
                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "rar",
                    Name = "RBAC Assessment Report",
                    Enabled = IsTrivyReportEnabled(
                        k8sOptions.Value.TrivyUseRbacAssessmentReport,
                        frOptions.Value.PvcName,
                        frOptions.Value.RbacAssessmentReportCrSubpath),
                },
                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "sr",
                    Name = "SBOM Report",
                    Enabled = IsTrivyReportEnabled(
                        k8sOptions.Value.TrivyUseSbomReport,
                        frOptions.Value.PvcName,
                        frOptions.Value.SbomReportCrSubpath),
                },
                new BackendSettingsTrivyReportConfigDto
                {
                    Id = "vr",
                    Name = "Vulnerability Report",
                    Enabled = IsTrivyReportEnabled(
                        k8sOptions.Value.TrivyUseVulnerabilityReport,
                        frOptions.Value.PvcName,
                        frOptions.Value.VulnerabilityReportCrSubpath),
                },

            ],
            UseDefaultContext = k8sOptions.Value.UseDefaultContext,
            IsUsedKubeConfigFileName = !string.IsNullOrWhiteSpace(k8sOptions.Value.KubeConfigFileName),
            IsUsedNamespaceList = !string.IsNullOrWhiteSpace(k8sOptions.Value.NamespaceList),
            IsUsedPvcName = !string.IsNullOrWhiteSpace(frOptions.Value.PvcName),
        };

        return Task.FromResult(backendSettingsDto);
    }

 
    private static bool IsTrivyReportEnabled(bool useTrivyReport, string pvcName, string subpath)
        => useTrivyReport == true && (string.IsNullOrWhiteSpace(pvcName) || (!string.IsNullOrWhiteSpace(pvcName) && !string.IsNullOrWhiteSpace(subpath)));
}
