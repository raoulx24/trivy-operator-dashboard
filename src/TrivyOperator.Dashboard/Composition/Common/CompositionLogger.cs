using Serilog;
using Serilog.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace TrivyOperator.Dashboard.Composition.Common;

internal static class CompositionLogger
{
    public static ILogger? Logger { get; private set; }

    public static void Initialize(IConfiguration configuration, string applicationName)
    {
        LoggerConfiguration loggerConfiguration =
            new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("ApplicationName", applicationName)
                .Enrich.WithMachineName()
                .Enrich.WithThreadId();

        Log.Logger = loggerConfiguration.CreateLogger();

        SerilogLoggerFactory serilogLoggerFactory =
            new(Log.Logger);

        Logger = serilogLoggerFactory.CreateLogger("Composition");
    }
}
