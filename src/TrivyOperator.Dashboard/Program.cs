using Serilog;
using TrivyOperator.Dashboard.Composition.Api;
using TrivyOperator.Dashboard.Composition.Common;
using TrivyOperator.Dashboard.Composition.Configuration;
using TrivyOperator.Dashboard.Composition.Health;
using TrivyOperator.Dashboard.Composition.History;
using TrivyOperator.Dashboard.Composition.Observability;
using TrivyOperator.Dashboard.Composition.TrivyOperatorDashboard;
using ILogger = Microsoft.Extensions.Logging.ILogger;

const string applicationName = "TrivyOperator.Dashboard";

Directory.SetCurrentDirectory(AppContext.BaseDirectory);

WebApplicationBuilder builder = WebApplication.CreateBuilder(
    new WebApplicationOptions
    {
        ApplicationName = applicationName,
        ContentRootPath = Directory.GetCurrentDirectory(),
        WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
    }
);

// configuration
IConfiguration configuration = TrivyDashboardConfigurationReader.CreateConfiguration();
builder.Configuration.Sources.Clear();
builder.Configuration.AddConfiguration(configuration);

// main app logger
ILogger logger = TrivyOperatorDashboardLogging.Configure(configuration, applicationName);
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(Log.Logger);
builder.Host.UseSerilog(Log.Logger);

// temp logger for composition
CompositionLogger.Initialize(logger);

// kestrel configurations
TrivyOperatorDashboardHostConfiguration.Configure(builder.WebHost, configuration, builder.Environment);

// check redis/valkey connectivity
await HistoryStartupChecks.CheckDistributedCacheConnectivity(configuration);

// api configurations (json options, swagger, headers, cors etc)
builder.Services.AddApiServices(configuration, builder.Environment);

// open telemetry
builder.Services.AddObservabilityServices(configuration, applicationName);

// health
builder.Services.AddHealthServices();

// all core business app services (trivy, alerts, backend settings etc)
builder.Services.AddTrivyOperatorDashboardServices(configuration);


WebApplication app = builder.Build();


// app lifecycle hooks (on started, on stopping etc)
TrivyOperatorDashboardLifecycle.Configure(app, logger);

// run history migrations
await app.RunHistoryMigrationsAsync();

// web app configurations
app.UseApiPipeline();
// open telemetry endpoints
app.MapObservabilityEndpoints(configuration);
// api endpoints
app.MapApiEndpoints();
// healthcheck endpoints
app.MapHealthEndpoints();


await app.RunAsync();

return 0;
