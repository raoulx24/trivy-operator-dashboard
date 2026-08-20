using TrivyOperator.Dashboard.Application.Alerts.Models;

namespace TrivyOperator.Dashboard.Application.Queries.Alerts.Models;

public class AlertDto
{
    public string Emitter { get; init; } = string.Empty;
    public string[] EmitterKey { get; init; } = [];
    public string Message { get; init; } = string.Empty;
    public string Severity { get; init; } = "Unknown";
    public string Category { get; init; } = "Unknown";
}

public static class AlertExtensions
{
    public static AlertDto ToAlertDto(this Alert alert, string emitter) => new()
    {
        Emitter = emitter,
        EmitterKey = [.. alert.Key.Value,],
        Message = alert.Message,
        Severity = alert.Severity.ToString(),
        Category = alert.Category,
    };
}
