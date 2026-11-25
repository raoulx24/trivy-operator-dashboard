namespace TrivyOperator.Dashboard.Application.BackendSettings.Queries.Models;

public sealed record BackendSettingsDto
{
    public List<BackendSettingsTrivyReportConfigDto> TrivyReportConfigDtos { get; init; } = [];
    public bool IsKubeConfigUsed { get; init; }
    public bool IsDefaultContextUsed { get; init; } = true;
    public bool IsNamespaceListUsed { get; init; }
    public bool IsFileRepositoryUsed { get; init; }
    public int VrHistoryMaxAgeDays { get; init; } = 14;
    // the effective state of the following - taken from the services, so false also when the feature is not composed
    // (e.g. no vulnerability reports are enabled)
    public bool IsCveDetailsEnabled { get; init; }
    public bool IsCveEnrichmentEnabled { get; init; }
    public bool IsVrHistoryEnabled { get; init; }
    public bool IsEolEnabled { get; init; }
    public bool IsScanOnDemandEnabled { get; init; }
    // the report types a scan on demand may collect (names); empty when not enabled
    public List<string> ScanOnDemandAllowedReportTypes { get; init; } = [];
    // why scan on demand is off although enabled in the settings; null otherwise
    public string? ScanOnDemandUnavailableReason { get; init; }
}

public sealed record BackendSettingsTrivyReportConfigDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool Enabled { get; init; }
}
