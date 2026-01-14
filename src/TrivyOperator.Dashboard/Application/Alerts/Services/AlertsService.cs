using Microsoft.AspNetCore.SignalR;
using TrivyOperator.Dashboard.Application.Alerts.Hubs;
using TrivyOperator.Dashboard.Application.Alerts.Models;
using TrivyOperator.Dashboard.Application.Alerts.Services.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.Abstractions;

namespace TrivyOperator.Dashboard.Application.Alerts.Services;

public class AlertsService(
    IConcurrentCache<string, Alert> cache,
    IHubContext<AlertsHub> alertsHubContext,
    ILogger<AlertsService> logger
) : IAlertsService
{
    public async Task AddAlert(string emitter, Alert alert, CancellationToken cancellationToken)
    {
        cache[GetCacheKey(emitter, alert.EmitterKey)] = alert;

        await alertsHubContext.Clients.All.SendAsync("ReceiveAddedAlert", alert.ToAlertDto(emitter), cancellationToken);

        logger.LogDebug(
            "Added alert for {emitter} and {emitterKey} with severity {alertSeverity}.",
            emitter,
            alert.EmitterKey,
            alert.Severity
        );
    }

    public async Task RemoveAlert(string emitter, Alert alert, CancellationToken cancellationToken)
    {
        cache.TryRemove(GetCacheKey(emitter, alert.EmitterKey), out _);

        await alertsHubContext.Clients.All.SendAsync(
            "ReceiveRemovedAlert",
            alert.ToAlertDto(emitter),
            cancellationToken
        );

        logger.LogDebug("Removed alert for {alertEmitter} and {emitterKey}.", emitter, alert.EmitterKey);
    }

    public Task<IEnumerable<AlertDto>> GetAlertDtos()
    {
        AlertDto[] result = [.. cache.Select(kvp => kvp.Value.ToAlertDto(kvp.Key.Split('|')[0])),];

        return Task.FromResult<IEnumerable<AlertDto>>(result);
    }

    private static string GetCacheKey(string emitter, string emmiterKey) => $"{emitter}|{emmiterKey}";
}
