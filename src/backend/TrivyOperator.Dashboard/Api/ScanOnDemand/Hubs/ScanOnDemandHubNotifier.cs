using Microsoft.AspNetCore.SignalR;
using TrivyOperator.Dashboard.Application.ScanOnDemand.Abstractions;

namespace TrivyOperator.Dashboard.Api.ScanOnDemand.Hubs;

// every client gets every change (with authentication later: one group per user)
public sealed class ScanOnDemandHubNotifier(
    IHubContext<ScanOnDemandHub> hubContext,
    ILogger<ScanOnDemandHubNotifier> logger
) : IOnDemandScanNotifier
{
    public async Task ScansChangedAsync(CancellationToken ctx = default)
    {
        try
        {
            await hubContext.Clients.All.SendAsync(ScanOnDemandHub.ScansChangedMethod, ctx);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "The scans change could not be pushed to the clients");
        }
    }
}
