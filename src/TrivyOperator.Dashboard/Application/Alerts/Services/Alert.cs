namespace TrivyOperator.Dashboard.Application.Alerts.Services;

public class Alert
{
    public string EmitterKey { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public Severity Severity { get; init; } = Severity.Info;
    public string Category { get; init; } = "Unknown";
}
