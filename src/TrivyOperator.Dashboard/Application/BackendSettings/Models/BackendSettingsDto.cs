namespace TrivyOperator.Dashboard.Application.BackendSettings.Models;

public class BackendSettingsDto
{
    public List<BackendSettingsTrivyReportConfigDto> TrivyReportConfigDtos { get; init; } = [];
    public bool IsKubeConfigUsed { get; init; } = false;
    public bool IsDefaultContextUsed { get; init; } = true;
    public bool IsNamespaceListUsed { get; init; } = false;
    public bool IsFileRepositoryUsed { get; init; } = false;
    public int VrHistoryMaxAgeDays { get; init; } = 14;
}

public class BackendSettingsTrivyReportConfigDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool Enabled { get; init; } = false;
}
