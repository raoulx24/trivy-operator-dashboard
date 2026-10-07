using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace TrivyOperator.Dashboard.Composition.TrivyOperatorDashboard;

public static class TrivyOperatorDashboardLifecycle
{
    public static void Configure(WebApplication app, ILogger? logger = null)
    {
        Logger = logger;
        
        app.Lifetime.ApplicationStarted.Register(OnStarted);
        app.Lifetime.ApplicationStopping.Register(OnStopping);
        app.Lifetime.ApplicationStopped.Register(OnStopped);
    }

    private static void OnStarted()
    {
        Logger?.LogInformation("OnStarted has been called.");
    }

    private static void OnStopping()
    {
        Logger?.LogInformation("OnStopping has been called.");
    }

    private static void OnStopped()
    {
        Logger?.LogInformation("OnStopped has been called.");
        Log.CloseAndFlush();
    }

    private static ILogger? Logger { get; set; }
}
