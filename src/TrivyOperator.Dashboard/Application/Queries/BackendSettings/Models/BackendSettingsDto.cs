namespace TrivyOperator.Dashboard.Application.Queries.BackendSettings.Models;

public class BackendSettingsDto
{
    public List<BackendSettingsTrivyReportConfigDto> TrivyReportConfigDtos { get; init; } = [];
    public bool IsKubeConfigUsed { get; init; }
    public bool IsDefaultContextUsed { get; init; } = true;
    public bool IsNamespaceListUsed { get; init; }
    public bool IsFileRepositoryUsed { get; init; }
    public int VrHistoryMaxAgeDays { get; init; } = 14;
}

public class BackendSettingsTrivyReportConfigDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool Enabled { get; init; }
}
