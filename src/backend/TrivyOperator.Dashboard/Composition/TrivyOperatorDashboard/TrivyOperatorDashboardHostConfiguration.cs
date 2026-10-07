using TrivyOperator.Dashboard.Composition.Configuration;

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

            int mainPort = configuration.GetPort("MainAppPort", 8900);
            

            options.ListenAnyIP(mainPort);

            string? configMetricsPort = configuration["OpenTelemetry:PrometheusExporterPort"];

            if (configMetricsPort is null)
            {
                return;
            }

            int metricsPort = configuration.GetPort("OpenTelemetry:PrometheusExporterPort", 8901);

            if (mainPort != metricsPort)
            {
                options.ListenAnyIP(metricsPort);
            }
        });
    }
}
