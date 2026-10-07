using Microsoft.AspNetCore.SignalR;
using TrivyOperator.Dashboard.Application.Alerts.Queries.Models;
using TrivyOperator.Dashboard.Application.Alerts.Queries.Services.Abstractions;

namespace TrivyOperator.Dashboard.Api.Alerts.Hubs;

public class AlertsHub(IAlertsService alertsService, ILogger<AlertsHub> logger) : Hub
{
    public override async Task OnConnectedAsync()
    {
        logger.LogDebug("New client connected to Hub.");
        IEnumerable<AlertDto> items = await alertsService.GetAlertDtos();
        foreach (AlertDto item in items)
        {
            await Clients.Caller.SendAsync("ReceiveAddedAlert", item);
        }

        await base.OnConnectedAsync();
    }
}
