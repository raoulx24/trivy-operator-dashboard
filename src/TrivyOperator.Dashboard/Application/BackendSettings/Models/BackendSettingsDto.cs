namespace TrivyOperator.Dashboard.Application.BackendSettings.Models;

public class BackendSettingsDto
{
    public List<BackendSettingsTrivyReportConfigDto> TrivyReportConfigDtos { get; init; } = [];
}

public class BackendSettingsTrivyReportConfigDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool Enabled { get; init; } = false;
}
