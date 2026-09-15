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

CompositionLogger.Initialize(builder.Configuration, applicationName);

IConfiguration configuration = TrivyDashboardConfigurationReader.CreateConfiguration();

builder.Configuration.Sources.Clear();
builder.Configuration.AddConfiguration(configuration);

ILogger logger = TrivyOperatorDashboardLogging.Configure(configuration, applicationName);

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(Log.Logger);
builder.Host.UseSerilog(Log.Logger);

TrivyOperatorDashboardHostConfiguration.Configure(builder.WebHost, configuration, builder.Environment);

await HistoryStartupChecks.CheckDistributedCacheConnectivity(configuration);

builder.Services.AddApiServices(configuration, builder.Environment);

builder.Services.AddObservabilityServices(configuration, applicationName);

builder.Services.AddTrivyOperatorDashboardServices(configuration);

WebApplication app = builder.Build();

TrivyOperatorDashboardLifecycle.Logger = logger;
TrivyOperatorDashboardLifecycle.Configure(app);

app.UseApiPipeline();

app.MapObservabilityEndpoints(configuration);
app.MapApiEndpoints();
app.MapHealthEndpoints();

await app.RunAsync();

return 0;
