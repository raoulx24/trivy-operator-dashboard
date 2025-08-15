﻿using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Application.Hubs;
using TrivyOperator.Dashboard.Application.Services.BuilderServicesExtensions;
using TrivyOperator.Dashboard.Utils;
using TrivyOperator.Dashboard.Utils.JsonConverters;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

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

IConfiguration configuration = CreateConfiguration();
builder.Configuration.Sources.Clear();
builder.Configuration.AddConfiguration(configuration);

LoggerConfiguration loggerConfiguration = new LoggerConfiguration().ReadFrom.Configuration(configuration);
loggerConfiguration.Enrich.FromLogContext();
loggerConfiguration.Enrich.WithMachineName();
loggerConfiguration.Enrich.WithThreadId();
loggerConfiguration.Enrich.WithProperty("Application", applicationName);
Log.Logger = loggerConfiguration.CreateLogger();
SerilogLoggerFactory serilogLoggerFactory = new(Log.Logger);
Logger = serilogLoggerFactory.CreateLogger<Program>();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(Log.Logger);

AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
TaskScheduler.UnobservedTaskException += TaskSchedulerUnobservedTaskException;

builder.Host.UseSerilog(Log.Logger);

builder.WebHost.UseShutdownTimeout(TimeSpan.FromSeconds(10));
builder.WebHost.ConfigureKestrel(options => { options.AddServerHeader = false; });

builder.Services.Configure<JsonOptions>(options => ConfigureJsonSerializerOptions(options.SerializerOptions));
builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                                   ForwardedHeaders.XForwardedProto |
                                   ForwardedHeaders.XForwardedHost;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    }
);

builder.Services.AddControllersWithViews(ConfigureMvcOptions)
    .AddJsonOptions(options => ConfigureJsonSerializerOptions(options.JsonSerializerOptions));
builder.Services.AddHttpClient();
builder.Services.AddProblemDetails();
if (!builder.Environment.IsProduction())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
        {
            options.SupportNonNullableReferenceTypes();
        }
    );
}

// SignalR: CORS with credentials must be allowed in order for cookie-based sticky sessions to work correctly. They must be enabled even when authentication isn't used.
builder.Services.AddCors(options => options.AddDefaultPolicy(configurePolicy =>
        configurePolicy.SetIsOriginAllowed(_ => true).AllowAnyHeader().AllowAnyMethod().AllowCredentials()
    )
);

builder.Services.AddCommons(configuration);
builder.Services.AddDomainServices();
builder.Services.AddAlertsServices();
builder.Services.AddWatcherStateServices();
builder.Services.AddV1NamespaceServices(configuration.GetSection("Kubernetes"));
builder.Services.AddClusterRbacAssessmentReportServices(configuration.GetSection("Kubernetes"));
builder.Services.AddConfigAuditReportServices(configuration.GetSection("Kubernetes"));
builder.Services.AddExposedSecretReportServices(configuration.GetSection("Kubernetes"));
builder.Services.AddVulnerabilityReportServices(configuration.GetSection("Kubernetes"));
builder.Services.AddClusterComplianceReportServices(configuration.GetSection("Kubernetes"));
builder.Services.AddClusterVulnerabilityReportServices(configuration.GetSection("Kubernetes"));
builder.Services.AddRbacAssessmentReportServices(configuration.GetSection("Kubernetes"));
builder.Services.AddSbomReportServices(configuration.GetSection("Kubernetes"));
builder.Services.AddClusterSbomReportServices(configuration.GetSection("Kubernetes"));
builder.Services.AddUiCommons();
builder.Services.AddOthers();
builder.Services.AddOpenTelemetry(
    configuration.GetSection("OpenTelemetry"),
    applicationName.Replace(".", string.Empty).ToLowerInvariant()
);

builder.WebHost.ConfigureKestrel(options =>
    {
        if (builder.Environment.IsProduction())
        {
            string? configMainPort = builder.Configuration["MainAppPort"];
            int mainPort = PortUtils.GetValidatedPort(configMainPort) ?? 8900;
            options.ListenAnyIP(mainPort);

            string? configMetricsPort = builder.Configuration["OpenTelemetry:PrometheusExporterPort"];
            if (configMetricsPort is not null)
            {
                int metricsPort = PortUtils.GetValidatedPort(configMetricsPort) ?? 8901;
                if (mainPort != metricsPort)
                {
                    options.ListenAnyIP(metricsPort);
                }
            }
        }
    }
);

WebApplication app = builder.Build();

IHostApplicationLifetime appLifetime = app.Lifetime;
appLifetime.ApplicationStarted.Register(OnStarted);
appLifetime.ApplicationStopping.Register(OnStopping);
appLifetime.ApplicationStopped.Register(OnStopped);

// Configure the HTTP request pipeline. Middleware order: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-9.0#middleware-order
app.UseForwardedHeaders();
if (app.Environment.IsProduction())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

//app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapStaticAssets();
app.UseRouting();
app.UseCors();
app.UseSerilogRequestLogging(options => options.GetLevel = (httpContext, _, _) =>
    httpContext.Request.Path.StartsWithSegments("/metrics") ? LogEventLevel.Verbose : LogEventLevel.Information
);
if (app.Environment.IsProduction())
{
    string? configMetricsPort = builder.Configuration["OpenTelemetry:PrometheusExporterPort"];
    if (configMetricsPort is not null)
    {
        int metricsPort = PortUtils.GetValidatedPort(configMetricsPort) ?? 8901;
        app.UseOpenTelemetryPrometheusScrapingEndpoint(context =>
            context.Request.Path == "/metrics" && context.Connection.LocalPort == metricsPort
        );
    }
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseOpenTelemetryPrometheusScrapingEndpoint();
}

app.MapControllers();
app.MapHub<AlertsHub>("/alerts-hub");
app.MapHealthChecks(
    "/healthz/live",
    new HealthCheckOptions
    {
        Predicate = check => check.Name == "watchers-liveness",
    }
);
app.MapHealthChecks(
    "/healthz/ready",
    new HealthCheckOptions
    {
        Predicate = check => check.Name == "watchers-readiness",
    }
);
app.MapFallbackToFile("index.html");

await app.RunAsync().ConfigureAwait(false);
return 0;

static IConfiguration CreateConfiguration()
{
    IConfigurationBuilder configurationBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json", true)
        .AddJsonFile("serilog.config.json", true)
        .AddEnvironmentVariables();
    IConfiguration configuration = configurationBuilder.Build();
    string? tempFolder = configuration.GetSection("FileExport")["TempFolder"];
    if (!string.IsNullOrEmpty(tempFolder))
    {
        return configuration;
    }

    Dictionary<string, string?> inMemorySettings = new()
    {
        {
            "FileExport:TempFolder", Path.GetTempPath()
        },
    };
    configurationBuilder.AddInMemoryCollection(inMemorySettings);
    configuration = configurationBuilder.Build();

    return configuration;
}

static void ConfigureJsonSerializerOptions(JsonSerializerOptions options)
{
    options.Converters.Add(new JsonStringEnumConverter());
    options.Converters.Add(new DateTimeJsonConverter());
    options.Converters.Add(new DateTimeNullableJsonConverter());
}

static void ConfigureMvcOptions(MvcOptions options)
{
    options.RespectBrowserAcceptHeader = true;
    options.OutputFormatters.Add(new XmlSerializerOutputFormatter());
    options.Filters.Add(new ProducesAttribute("application/json"));
}

static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
{
    if (e.ExceptionObject is Exception ex)
    {
        Logger?.LogError(ex, "UnhandledException");
    }
    else
    {
        string? msg = e.ExceptionObject.ToString();
        int exCode = Marshal.GetLastWin32Error();
        if (exCode != 0)
        {
            msg += " ErrorCode: " + exCode.ToString("X16");
        }

        Logger?.LogError("Unhandled External Exception: {msg}", msg);
    }
}

static void TaskSchedulerUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
{
    Logger?.LogError(e.Exception, "ERROR: UNOBSERVED TASK EXCEPTION");
    e.SetObserved();
}

static void OnStarted() => Logger?.LogInformation("OnStarted has been called.");

static void OnStopping() => Logger?.LogInformation("OnStopping has been called.");

static void OnStopped()
{
    Logger?.LogInformation("OnStopped has been called.");
    Log.CloseAndFlush();
}

internal partial class Program
{
    private static ILogger? Logger { get; set; }
}
