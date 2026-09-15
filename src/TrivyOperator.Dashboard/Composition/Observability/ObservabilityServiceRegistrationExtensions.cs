using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;
using TrivyOperator.Dashboard.Infrastructure.Clients.Metrics;
using TrivyOperator.Dashboard.Infrastructure.Clients.Metrics.Abstractions;

namespace TrivyOperator.Dashboard.Composition.Observability;

public static class ObservabilityServiceRegistrationExtensions
{
    public static void AddObservabilityServices(
        this IServiceCollection services,
        IConfiguration configuration,
        string applicationName)
    {
        services.AddOpenTelemetry(
            configuration.GetSection("OpenTelemetry"),
            applicationName.Replace(".", string.Empty).ToLowerInvariant());
    }
    
    public static void AddOpenTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        string applicationName
    )
    {
        bool? a = configuration.GetValue<bool?>("Enabled");
        if (configuration.GetValue<bool?>("Enabled") ?? false)
        {
            services.AddSingleton<IMetricsClient>(_ => new MetricsClient(applicationName));

            // string fileVersion =
            //     Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "0.0";
            // this is AOT friendly
            // TODO: verify build in github
            string fileVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0";

            string? otelEndpoint = configuration.GetValue<string>("OtelEndpoint");
            bool? isConsoleEnabled = configuration.GetValue<bool?>("ConsoleEnabled");
            bool? isAspNetCoreEnabled = configuration.GetValue<bool?>("AspNetCoreInstrumentationEnabled");
            bool? isRuntimeEnabled = configuration.GetValue<bool?>("RuntimeInstrumentationEnabled");
            int? metricsPort = configuration.GetValue<int>("PrometheusExporterPort");
            double[] histogramBounds =
                configuration.GetValue<double[]>("HistogramBoundsInMs") ?? [200, 500, 1000, 5000,];

            services.AddSingleton<IMetricsClient>(_ => new MetricsClient(applicationName));
            services.AddOpenTelemetry()
                .WithTracing(tracingBuilder =>
                    {
                        tracingBuilder.SetResourceBuilder(
                                ResourceBuilder.CreateDefault()
                                    .AddService(applicationName)
                                    .AddAttributes(
                                        new Dictionary<string, object>
                                        {
                                            {
                                                "service.version", fileVersion
                                            },
                                        }
                                    )
                            )
                            .AddHttpClientInstrumentation();
                        if (isConsoleEnabled ?? false)
                        {
                            tracingBuilder.AddConsoleExporter();
                        }

                        if (!string.IsNullOrWhiteSpace(otelEndpoint))
                        {
                            tracingBuilder.AddOtlpExporter(options =>
                                {
                                    options.Endpoint = new Uri(otelEndpoint);
                                    options.Protocol =
                                        (configuration.GetValue<string?>("OtelProtocol")?.ToLowerInvariant() ??
                                         "grpc") ==
                                        "grpc"
                                            ? OtlpExportProtocol.Grpc : OtlpExportProtocol.HttpProtobuf;
                                }
                            );
                        }

                        if (isAspNetCoreEnabled ?? false)
                        {
                            tracingBuilder.AddAspNetCoreInstrumentation(options =>
                                {
                                    options.Filter = context =>
                                    {
                                        string? path = context.Request.Path.Value;
                                        return !((path?.StartsWith("/healthz") ?? false) ||
                                                 (path?.StartsWith("/metrics") ?? false));
                                    };
                                }
                            );
                        }
                    }
                )
                .WithMetrics(metricsBuilder =>
                    {
                        metricsBuilder.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(applicationName))
                            .AddView(
                                "*",
                                new ExplicitBucketHistogramConfiguration
                                {
                                    Boundaries = histogramBounds,
                                    // defaults: [ 0, 5, 10, 25, 50, 75, 100, 250, 500, 750, 1000, 2500, 5000, 7500, 10000 ]
                                }
                            )
                            .AddMeter($"{applicationName}.metrics");
                        if (!string.IsNullOrWhiteSpace(otelEndpoint))
                        {
                            metricsBuilder.AddOtlpExporter(options =>
                                {
                                    options.Endpoint = new Uri(otelEndpoint);
                                    options.Protocol =
                                        (configuration.GetValue<string?>("OTelProtocol")?.ToLowerInvariant() ??
                                         "grpc") ==
                                        "grpc"
                                            ? OtlpExportProtocol.Grpc : OtlpExportProtocol.HttpProtobuf;
                                }
                            );
                        }

                        if (isConsoleEnabled ?? false)
                        {
                            metricsBuilder.AddConsoleExporter();
                        }

                        if (isAspNetCoreEnabled ?? false)
                        {
                            metricsBuilder.AddAspNetCoreInstrumentation();
                        }

                        if (isRuntimeEnabled ?? false)
                        {
                            metricsBuilder.AddRuntimeInstrumentation();
                        }

                        if (metricsPort is not null)
                        {
                            metricsBuilder.AddPrometheusExporter();
                        }
                    }
                );
        }
    }
}
