using Serilog;
using Serilog.Extensions.Logging;
using System.Runtime.InteropServices;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace TrivyOperator.Dashboard.Composition.Configuration;

public static class TrivyOperatorDashboardLogging
{
    internal static ILogger Configure(IConfiguration configuration, string applicationName)
    {
        LoggerConfiguration loggerConfiguration = new LoggerConfiguration().ReadFrom.Configuration(configuration);

        loggerConfiguration.Enrich.FromLogContext();
        loggerConfiguration.Enrich.WithMachineName();
        loggerConfiguration.Enrich.WithThreadId();
        loggerConfiguration.Enrich.WithProperty("Application", applicationName);

        Log.Logger = loggerConfiguration.CreateLogger();

        SerilogLoggerFactory serilogLoggerFactory = new(Log.Logger);

        ILogger logger = serilogLoggerFactory.CreateLogger<Program>();

        AppDomain.CurrentDomain.UnhandledException +=
            (_, e) => HandleUnhandledException(logger, e);

        TaskScheduler.UnobservedTaskException +=
            (_, e) => HandleUnobservedTaskException(logger, e);

        return logger;
    }

    private static void HandleUnhandledException(
        ILogger logger,
        UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            logger.LogError(ex, "UnhandledException");
            return;
        }

        string? msg = e.ExceptionObject.ToString();
        int exCode = Marshal.GetLastWin32Error();

        if (exCode != 0)
        {
            msg += " ErrorCode: " + exCode.ToString("X16");
        }

        logger.LogError("Unhandled External Exception: {msg}", msg);
    }

    private static void HandleUnobservedTaskException(ILogger logger, UnobservedTaskExceptionEventArgs e)
    {
        logger.LogError(e.Exception, "ERROR: UNOBSERVED TASK EXCEPTION");

        e.SetObserved();
    }
}
