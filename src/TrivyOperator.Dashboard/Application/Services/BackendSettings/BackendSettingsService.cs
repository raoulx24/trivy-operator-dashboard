using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.Models;
using TrivyOperator.Dashboard.Application.Services.BackendSettings.Abstractions;
using TrivyOperator.Dashboard.Application.Services.Options;

namespace TrivyOperator.Dashboard.Application.Services.BackendSettings;

public class BackendSettingsService(IOptions<KubernetesOptions> options) : IBackendSettingsService
{
    public Task<BackendSettingsDto> GetBackendSettings()
    {
        BackendSettingsDto backendSettingsDto = new() { TrivyReportConfigDtos =
        [
            new BackendSettingsTrivyReportConfigDto
            {
                Id = "ccr",
                Name = "Cluster Compliance Report",
                Enabled = options.Value.TrivyUseClusterComplianceReport,
            },
            new BackendSettingsTrivyReportConfigDto
            {
                Id = "crar",
                Name = "Cluster RBAC Assessment Report",
                Enabled = options.Value.TrivyUseClusterRbacAssessmentReport,
            },
            new BackendSettingsTrivyReportConfigDto
            {
                Id = "csr",
                Name = "Cluster SBOM Report",
                Enabled = options.Value.TrivyUseClusterSbomReport,
            },
            new BackendSettingsTrivyReportConfigDto
            {
                Id = "cvr",
                Name = "Cluster Vulnerability Report",
                Enabled = options.Value.TrivyUseClusterVulnerabilityReport,
            },

            new BackendSettingsTrivyReportConfigDto
            {
                Id = "car",
                Name = "Config Audit Report",
                Enabled = options.Value.TrivyUseConfigAuditReport,
            },
            new BackendSettingsTrivyReportConfigDto
            {
                Id = "esr",
                Name = "Exposed Secret Report",
                Enabled = options.Value.TrivyUseExposedSecretReport,
            },
            new BackendSettingsTrivyReportConfigDto
            {
                Id = "iar",
                Name = "Infra Assessment Report",
                Enabled = options.Value.TrivyUseInfraAssessmentReport,
            },
            new BackendSettingsTrivyReportConfigDto
            {
                Id = "rar",
                Name = "RBAC Assessment Report",
                Enabled = options.Value.TrivyUseRbacAssessmentReport,
            },
            new BackendSettingsTrivyReportConfigDto
            {
                Id = "sr",
                Name = "SBOM Report",
                Enabled = options.Value.TrivyUseSbomReport,
            },
            new BackendSettingsTrivyReportConfigDto
            {
                Id = "vr",
                Name = "Vulnerability Report",
                Enabled = options.Value.TrivyUseVulnerabilityReport,
            },

        ], };

        return Task.FromResult(backendSettingsDto);
    }
}
