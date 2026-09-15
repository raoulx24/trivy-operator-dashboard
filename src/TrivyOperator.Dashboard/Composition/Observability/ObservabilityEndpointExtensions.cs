using TrivyOperator.Dashboard.Application.Utils;

namespace TrivyOperator.Dashboard.Composition.Observability;

public static class ObservabilityEndpointExtensions
{
    public static void MapObservabilityEndpoints(
        this WebApplication app,
        IConfiguration configuration)
    {
        if (app.Environment.IsProduction())
        {
            string? configMetricsPort =
                configuration["OpenTelemetry:PrometheusExporterPort"];

            if (configMetricsPort is not null)
            {
                int metricsPort =
                    PortUtils.GetValidatedPort(configMetricsPort) ?? 8901;

                app.UseOpenTelemetryPrometheusScrapingEndpoint(
                    context =>
                        context.Request.Path == "/metrics" &&
                        context.Connection.LocalPort == metricsPort);
            }
        }
        else
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseOpenTelemetryPrometheusScrapingEndpoint();
        }
    }
}
