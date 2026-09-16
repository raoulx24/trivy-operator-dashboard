using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Text.Json;
using TrivyOperator.Dashboard.Api.Alerts.Serializations;
using TrivyOperator.Dashboard.Api.AppVersions.Serializations;
using TrivyOperator.Dashboard.Api.BackendSettings.Serializations;
using TrivyOperator.Dashboard.Api.History.Serializations;
using TrivyOperator.Dashboard.Api.Serialization;
using TrivyOperator.Dashboard.Api.Trivy.Serializations;
using TrivyOperator.Dashboard.Infrastructure.Shared.JsonConverters;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;
using KubernetesApiJsonContext = TrivyOperator.Dashboard.Api.Kubernetes.Serializations.KubernetesApiJsonContext;
using KubernetesNamespacesApiJsonContext = TrivyOperator.Dashboard.Api.Kubernetes.Serializations.KubernetesNamespacesApiJsonContext;
using WatcherStatusApiJsonContext = TrivyOperator.Dashboard.Api.Kubernetes.Serializations.WatcherStatusApiJsonContext;


namespace TrivyOperator.Dashboard.Composition.Api;

public static class ApiServiceRegistrationExtensions
{
    public static void AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.Configure<JsonOptions>(options => ConfigureJson(options.SerializerOptions));

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor |
                ForwardedHeaders.XForwardedProto |
                ForwardedHeaders.XForwardedHost;

            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

        services.AddHttpClient();
        services.AddHttpContextAccessor();
        services.AddProblemDetails();

        services.AddCors(options =>
            options.AddDefaultPolicy(configurePolicy =>
                configurePolicy
                    .SetIsOriginAllowed(_ => true)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()));

        if (!environment.IsProduction())
        {
            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(options =>
            {
                options.SupportNonNullableReferenceTypes();
            });
        }

        services
            .AddControllersWithViews(ConfigureMvcOptions)
            .AddJsonOptions(options =>
                ConfigureJson(options.JsonSerializerOptions));
    }

    private static void ConfigureJson(JsonSerializerOptions options)
    {
        options.TypeInfoResolverChain.Add(ControllersApiJsonContext.Default);

        options.TypeInfoResolverChain.Add(AlertsApiJsonContext.Default);
        options.TypeInfoResolverChain.Add(AppVersionsApiJsonContext.Default);
        options.TypeInfoResolverChain.Add(BackendSettingsApiJsonContext.Default);
        options.TypeInfoResolverChain.Add(VulnerabilityReportsHistoryApiJsonContext.Default);
        options.TypeInfoResolverChain.Add(KubernetesApiJsonContext.Default);
        options.TypeInfoResolverChain.Add(KubernetesNamespacesApiJsonContext.Default);
        options.TypeInfoResolverChain.Add(WatcherStatusApiJsonContext.Default);
        options.TypeInfoResolverChain.Add(ClusterComplianceReportsApiJsonContext.Default);
        options.TypeInfoResolverChain.Add(ClusterConfigAuditReportApiJsonContext.Default);
        options.TypeInfoResolverChain.Add(ClusterInfraAssessmentReportApiJsonContext.Default);
        options.TypeInfoResolverChain.Add(ClusterRbacAssessmentReportApiJsonContext.Default);
        options.TypeInfoResolverChain.Add(ClusterSbomReportApiJsonContext.Default);
        options.TypeInfoResolverChain.Add(ClusterVulnerabilityReportApiJsonContext.Default);
        options.TypeInfoResolverChain.Add(ConfigAuditReportApiJsonContext.Default);
        options.TypeInfoResolverChain.Add(ExposedSecretReportsApiJsonContext.Default);
        options.TypeInfoResolverChain.Add(InfraAssessmentReportApiJsonContext.Default);
        options.TypeInfoResolverChain.Add(RbacAssessmentReportApiJsonContext.Default);
        options.TypeInfoResolverChain.Add(SbomReportApiJsonContext.Default);
        options.TypeInfoResolverChain.Add(SeveritiesApiJsonContext.Default);
        options.TypeInfoResolverChain.Add(TrivyReportDependenciesApiJsonContext.Default);
        options.TypeInfoResolverChain.Add(VulnerabilityReportsApiJsonContext.Default);

        options.Converters.Add(new DateTimeJsonConverter());
        options.Converters.Add(new DateTimeNullableJsonConverter());
    }

    private static void ConfigureMvcOptions(MvcOptions options)
    {
        options.RespectBrowserAcceptHeader = true;
        options.OutputFormatters.Add(new XmlSerializerOutputFormatter());
        options.Filters.Add(new ProducesAttribute("application/json"));
    }
}
