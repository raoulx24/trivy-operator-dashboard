using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using StackExchange.Redis;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Application.Alerts.Hubs;
using TrivyOperator.Dashboard.Application.Common;
using TrivyOperator.Dashboard.Application.Utils;
using TrivyOperator.Dashboard.Domain.Utils.JsonConverters;
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

ConfigureLogging(configuration);

// check distributed cache server availability and fail if not available
await CheckDistributedCacheConnectivity(configuration);

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(Log.Logger);

builder.Host.UseSerilog(Log.Logger);

builder.WebHost.UseShutdownTimeout(TimeSpan.FromSeconds(10));
builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

builder.Services.Configure<JsonOptions>(options => ConfigureJsonSerializerOptions(options.SerializerOptions));
builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                                   ForwardedHeaders.XForwardedProto |
                                   ForwardedHeaders.XForwardedHost;
        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();
    }
);
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.AddProblemDetails();
// SignalR: CORS with credentials must be allowed in order for cookie-based sticky sessions to work correctly. They must be enabled even when authentication isn't used.
builder.Services.AddCors(options => options.AddDefaultPolicy(configurePolicy =>
        configurePolicy.SetIsOriginAllowed(_ => true).AllowAnyHeader().AllowAnyMethod().AllowCredentials()
    )
);
if (!builder.Environment.IsProduction())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
        {
            options.SupportNonNullableReferenceTypes();
        }
    );
}

builder.Services.AddControllersWithViews(ConfigureMvcOptions)
    .AddJsonOptions(options => ConfigureJsonSerializerOptions(options.JsonSerializerOptions));
builder.Services.AddCommons(configuration);
builder.Services.AddAlertsServices();
builder.Services.AddWatcherStateServices();
builder.Services.AddHistoryServices(configuration);
builder.Services.AddV1NamespaceServices(configuration);
builder.Services.AddTrivyServices(configuration);

builder.Services.AddUiCommons();
builder.Services.AddOthers();
builder.Services.AddOpenTelemetry(
    configuration.GetSection("OpenTelemetry"),
    applicationName.Replace(".", string.Empty).ToLowerInvariant()
);

builder.WebHost.ConfigureKestrel(options =>
    {
        if (!builder.Environment.IsProduction())
        {
            return;
        }

        string? configMainPort = builder.Configuration["MainAppPort"];
        int mainPort = PortUtils.GetValidatedPort(configMainPort) ?? 8900;
        options.ListenAnyIP(mainPort);

        string? configMetricsPort = builder.Configuration["OpenTelemetry:PrometheusExporterPort"];
        if (configMetricsPort is null)
        {
            return;
        }

        int metricsPort = PortUtils.GetValidatedPort(configMetricsPort) ?? 8901;
        if (mainPort != metricsPort)
        {
            options.ListenAnyIP(metricsPort);
        }
    }
);

WebApplication app = builder.Build();

app.Lifetime.ApplicationStarted.Register(OnStarted);
app.Lifetime.ApplicationStopping.Register(OnStopping);
app.Lifetime.ApplicationStopped.Register(OnStopped);

// Configure the HTTP request pipeline. Middleware order: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-10.0#middleware-order
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

// Apps deployed in a reverse proxy configuration allow the proxy to handle connection security (HTTPS). If the proxy also handles HTTPS redirection, there's no need to use HTTPS Redirection Middleware.
//app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles(
    new StaticFileOptions
    {
        OnPrepareResponse = ctx =>
        {
            IHeaderDictionary headers = ctx.Context.Response.Headers;
            string? path = ctx.File.PhysicalPath;
            if (path?.EndsWith("index.html") ?? false)
            {
                headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
                headers.Append("Pragma", "no-cache");
                headers.Append("Expires", "0");
            }
            else
            {
                headers.Append("Cache-Control", "public,max-age=2592000,immutable");
            }
        },
    }
);
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

await app.RunAsync();

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

static void ConfigureLogging(IConfiguration configuration)
{
    LoggerConfiguration loggerConfiguration = new LoggerConfiguration().ReadFrom.Configuration(configuration);
    loggerConfiguration.Enrich.FromLogContext();
    loggerConfiguration.Enrich.WithMachineName();
    loggerConfiguration.Enrich.WithThreadId();
    loggerConfiguration.Enrich.WithProperty("Application", applicationName);
    Log.Logger = loggerConfiguration.CreateLogger();
    SerilogLoggerFactory serilogLoggerFactory = new(Log.Logger);
    Logger = serilogLoggerFactory.CreateLogger<Program>();
    BuilderServicesExtensions.Logger = Logger;
    AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
    TaskScheduler.UnobservedTaskException += TaskSchedulerUnobservedTaskException;
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

static void OnStarted()
{
    Logger?.LogInformation("OnStarted has been called.");
}

static void OnStopping()
{
    Logger?.LogInformation("OnStopping has been called.");
}

static void OnStopped()
{
    Logger?.LogInformation("OnStopped has been called.");
    Log.CloseAndFlush();
}

static async Task CheckDistributedCacheConnectivity(IConfiguration configuration)
{
    bool isHistoryEnabled = configuration.GetValue<bool?>("History:Enabled") ?? false;
    bool useDefaultContext = configuration.GetValue<bool?>("Kubernetes:UseDefaultContext") ?? false;
    bool useFileRepository = !string.IsNullOrWhiteSpace(configuration.GetValue<string?>("FileRepository:BasePath"));

    bool shouldUseRedis = isHistoryEnabled && useDefaultContext && !useFileRepository;

    if (!shouldUseRedis)
        return;

    string connString = configuration.GetValue<string?>("History:DistributedCache:ConnectionString")
        ?? throw new InvalidOperationException("Distributed Cache connection string missing.");

    TimeSpan timeout = TimeSpan.FromSeconds(60);
    TimeSpan delay = TimeSpan.FromSeconds(1);

    using CancellationTokenSource overallCts = new(timeout);

    while (!overallCts.Token.IsCancellationRequested)
    {
        try
        {
            using CancellationTokenSource connectCts = CancellationTokenSource.CreateLinkedTokenSource(overallCts.Token);
            connectCts.CancelAfter(TimeSpan.FromSeconds(5)); // per-attempt timeout

            ConnectionMultiplexer conn = await ConnectionMultiplexer.ConnectAsync(connString);
            await conn.GetDatabase().PingAsync();

            conn.Dispose();

            Logger?.LogInformation("Distributed Cache connectivity check succeeded.");
            return;
        }
        catch (OperationCanceledException) when (overallCts.IsCancellationRequested)
        {
            break;
        }
        catch (Exception ex)
        {
            Logger?.LogWarning(ex, "Distributed Cache (Redis/Valkey) is not reachable, retrying in {Delay}", delay);

            try
            {
                await Task.Delay(delay, overallCts.Token);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            // simple backoff
            delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 5));
        }
    }

    throw new InvalidOperationException("Distributed Cache server is not reachable after retries.");
}

internal partial class Program
{
    private static ILogger? Logger { get; set; }
}
