using TrivyOperator.Dashboard.Api.Alerts.Hubs;

namespace TrivyOperator.Dashboard.Composition.Api;

public static class ApiEndpointExtensions
{
    public static void MapApiEndpoints(this WebApplication app)
    {
        app.MapControllers();
        app.MapHub<AlertsHub>("/alerts-hub");
        app.MapFallbackToFile("index.html");
    }
}
