namespace TrivyOperator.Dashboard.Application.BackendSettings.Models;

public class BackendSettingsDto
{
    public List<BackendSettingsTrivyReportConfigDto> TrivyReportConfigDtos { get; init; } = [];
    public bool IsUsedKubeConfigFileName { get; init; } = false;
    public bool UseDefaultContext { get; init; } = true;
    public bool IsUsedNamespaceList { get; init; } = false;
    public bool IsUsedPvcName { get; init; } = false;
}

public class BackendSettingsTrivyReportConfigDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool Enabled { get; init; } = false;
}
