using Microsoft.AspNetCore.SignalR;
using TrivyOperator.Dashboard.Api.Alerts.Hubs;
using TrivyOperator.Dashboard.Api.Alerts.Models;
using TrivyOperator.Dashboard.Application.Alerts.Abstractions;
using TrivyOperator.Dashboard.Application.Alerts.Models;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemoryOld.Abstractions;

namespace TrivyOperator.Dashboard.Api.Alerts.Services;

public class AlertPublisher(
    IConcurrentCache<AlertKey, Alert> cache,
    IHubContext<AlertsHub> alertsHubContext,
    ILogger<AlertsService> logger) : IAlertPublisher
{
    public async Task AddAlert(string emitter, Alert alert, CancellationToken cancellationToken)
    {
        AlertKey key = new(emitter, alert.Key);
        cache[key] = alert;

        await alertsHubContext.Clients.All.SendAsync("ReceiveAddedAlert", alert.ToAlertDto(emitter), cancellationToken);

        logger.LogDebug(
            "Added alert for {emitter} and {emitterKey} with severity {alertSeverity}.",
            emitter,
            alert.Key,
            alert.Severity
        );
    }

    public async Task RemoveAlert(string emitter, Alert alert, CancellationToken cancellationToken)
    {
        AlertKey key = new(emitter, alert.Key);
        cache.TryRemove(key, out _);

        await alertsHubContext.Clients.All.SendAsync(
            "ReceiveRemovedAlert",
            alert.ToAlertDto(emitter),
            cancellationToken
        );

        logger.LogDebug("Removed alert for {alertEmitter} and {emitterKey}.", emitter, alert.Key);
    }
}
