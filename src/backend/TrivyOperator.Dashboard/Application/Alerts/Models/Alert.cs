namespace TrivyOperator.Dashboard.Application.Alerts.Models;

public class Alert
{
    public EmitterKey Key { get; init; } = new();
    public string Message { get; init; } = string.Empty;
    public Severity Severity { get; init; } = Severity.Info;
    public string Category { get; init; } = "Unknown";
}
