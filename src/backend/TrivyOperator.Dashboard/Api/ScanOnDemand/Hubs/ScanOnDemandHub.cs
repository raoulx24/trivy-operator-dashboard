using Microsoft.AspNetCore.SignalR;

namespace TrivyOperator.Dashboard.Api.ScanOnDemand.Hubs;

// push only: the clients listen to "ScansChanged"; there are no server methods
public class ScanOnDemandHub : Hub
{
    public const string ScansChangedMethod = "ScansChanged";
}
