using TrivyOperator.Dashboard.Api.Alerts.Hubs;
using TrivyOperator.Dashboard.Api.ScanOnDemand.Hubs;

namespace TrivyOperator.Dashboard.Composition.Api;

public static class ApiEndpointExtensions
{
    public static void MapApiEndpoints(this WebApplication app)
    {
        app.MapControllers();
        app.MapHub<AlertsHub>("/alerts-hub");
        // mapped also when scan on demand is off: it has no server methods, and the frontend connects only when enabled
        app.MapHub<ScanOnDemandHub>("/scan-on-demand-hub");
        app.MapFallbackToFile("index.html");
    }
}
