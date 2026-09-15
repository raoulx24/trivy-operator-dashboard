using TrivyOperator.Dashboard.Composition.Common;

namespace TrivyOperator.Dashboard.Composition.TrivyOperatorDashboard;

public static class TrivyOperatorDashboardHostConfiguration
{
    internal static void Configure(
        IWebHostBuilder webHost,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        webHost.UseShutdownTimeout(TimeSpan.FromSeconds(10));

        webHost.ConfigureKestrel(options =>
        {
            options.AddServerHeader = false;

            if (!environment.IsProduction())
            {
                return;
            }

            string? configMainPort = configuration["MainAppPort"];
            int mainPort = PortUtils.GetValidatedPort(configMainPort) ?? 8900;

            options.ListenAnyIP(mainPort);

            string? configMetricsPort =
                configuration["OpenTelemetry:PrometheusExporterPort"];

            if (configMetricsPort is null)
            {
                return;
            }

            int metricsPort = PortUtils.GetValidatedPort(configMetricsPort) ?? 8901;

            if (mainPort != metricsPort)
            {
                options.ListenAnyIP(metricsPort);
            }
        });
    }
}
