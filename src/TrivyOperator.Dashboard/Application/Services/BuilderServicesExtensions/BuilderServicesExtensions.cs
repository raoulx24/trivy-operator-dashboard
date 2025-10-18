using k8s.Models;
using Microsoft.Extensions.Diagnostics.Metrics;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;
using TrivyOperator.Dashboard.Application.HealthChecks;
using TrivyOperator.Dashboard.Application.Internals;
using TrivyOperator.Dashboard.Application.Services.Alerts;
using TrivyOperator.Dashboard.Application.Services.Alerts.Abstractions;
using TrivyOperator.Dashboard.Application.Services.AppVersions;
using TrivyOperator.Dashboard.Application.Services.AppVersions.Abstractions;
using TrivyOperator.Dashboard.Application.Services.BackendSettings;
using TrivyOperator.Dashboard.Application.Services.BackendSettings.Abstractions;
using TrivyOperator.Dashboard.Application.Services.BackgroundQueues;
using TrivyOperator.Dashboard.Application.Services.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Application.Services.CacheRefreshers;
using TrivyOperator.Dashboard.Application.Services.KubernetesEventCoordinators;
using TrivyOperator.Dashboard.Application.Services.KubernetesEventCoordinators.Abstractions;
using TrivyOperator.Dashboard.Application.Services.KubernetesEventDispatchers;
using TrivyOperator.Dashboard.Application.Services.KubernetesEventDispatchers.Abstractions;
using TrivyOperator.Dashboard.Application.Services.Namespaces;
using TrivyOperator.Dashboard.Application.Services.Namespaces.Abstractions;
using TrivyOperator.Dashboard.Application.Services.Options;
using TrivyOperator.Dashboard.Application.Services.RawDomainQueryServices;
using TrivyOperator.Dashboard.Application.Services.RawDomainQueryServices.Abstracts;
using TrivyOperator.Dashboard.Application.Services.Trivy.ClusterComplianceReport;
using TrivyOperator.Dashboard.Application.Services.Trivy.ClusterComplianceReport.Abstractions;
using TrivyOperator.Dashboard.Application.Services.Trivy.ClusterRbacAssessmentReport;
using TrivyOperator.Dashboard.Application.Services.Trivy.ClusterRbacAssessmentReport.Abstractions;
using TrivyOperator.Dashboard.Application.Services.Trivy.ClusterSbomReport;
using TrivyOperator.Dashboard.Application.Services.Trivy.ClusterSbomReport.Abstractions;
using TrivyOperator.Dashboard.Application.Services.Trivy.ClusterVulnerabilityReport;
using TrivyOperator.Dashboard.Application.Services.Trivy.ClusterVulnerabilityReport.Abstractions;
using TrivyOperator.Dashboard.Application.Services.Trivy.ConfigAuditReport;
using TrivyOperator.Dashboard.Application.Services.Trivy.ConfigAuditReport.Abstractions;
using TrivyOperator.Dashboard.Application.Services.Trivy.ExposedSecretReport;
using TrivyOperator.Dashboard.Application.Services.Trivy.ExposedSecretReport.Abstractions;
using TrivyOperator.Dashboard.Application.Services.Trivy.RbacAssessmentReport;
using TrivyOperator.Dashboard.Application.Services.Trivy.RbacAssessmentReport.Abstractions;
using TrivyOperator.Dashboard.Application.Services.Trivy.SbomReport;
using TrivyOperator.Dashboard.Application.Services.Trivy.SbomReport.Abstractions;
using TrivyOperator.Dashboard.Application.Services.Trivy.VulnerabilityReport;
using TrivyOperator.Dashboard.Application.Services.Trivy.VulnerabilityReport.Abstractions;
using TrivyOperator.Dashboard.Application.Services.TrivyReportDependencies;
using TrivyOperator.Dashboard.Application.Services.TrivyReportDependencies.Abstractions;
using TrivyOperator.Dashboard.Application.Services.WatcherEvents;
using TrivyOperator.Dashboard.Application.Services.Watchers;
using TrivyOperator.Dashboard.Application.Services.Watchers.Abstractions;
using TrivyOperator.Dashboard.Application.Services.WatcherStateAlertRefreshers;
using TrivyOperator.Dashboard.Application.Services.WatcherStates;
using TrivyOperator.Dashboard.Application.Services.WatcherStates.Abstractions;
using TrivyOperator.Dashboard.Domain.Services;
using TrivyOperator.Dashboard.Domain.Services.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.ClusterComplianceReport;
using TrivyOperator.Dashboard.Domain.Trivy.ClusterRbacAssessmentReport;
using TrivyOperator.Dashboard.Domain.Trivy.ClusterSbomReport;
using TrivyOperator.Dashboard.Domain.Trivy.ClusterVulnerabilityReport;
using TrivyOperator.Dashboard.Domain.Trivy.ConfigAuditReport;
using TrivyOperator.Dashboard.Domain.Trivy.CustomResources.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.ExposedSecretReport;
using TrivyOperator.Dashboard.Domain.Trivy.RbacAssessmentReport;
using TrivyOperator.Dashboard.Domain.Trivy.SbomReport;
using TrivyOperator.Dashboard.Domain.Trivy.VulnerabilityReport;
using TrivyOperator.Dashboard.Infrastructure.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Clients;
using TrivyOperator.Dashboard.Infrastructure.Clients.Models;
using TrivyOperator.Dashboard.Infrastructure.Services;

namespace TrivyOperator.Dashboard.Application.Services.BuilderServicesExtensions;

public static class BuilderServicesExtensions
{
    public static void AddV1NamespaceServices(
        this IServiceCollection services, IConfiguration kubernetesConfiguration)
    {
        services
            .AddSingleton<IConcurrentDictionaryCache<V1Namespace>, ConcurrentDictionaryCache<V1Namespace>>();
        services.AddSingleton<IKubernetesBackgroundQueue<V1Namespace>, KubernetesBackgroundQueue<V1Namespace>>();
        if (string.IsNullOrWhiteSpace(kubernetesConfiguration.GetValue<string>("NamespaceList")))
        {
            services.AddSingleton<NamespaceDomainService>();
            services.AddSingleton<IClusterScopedResourceQueryDomainService<V1Namespace, V1NamespaceList>>(
                sp => sp.GetRequiredService<NamespaceDomainService>());
            services.AddSingleton<IClusterScopedResourceWatchDomainService<V1Namespace, V1NamespaceList>>(
                sp => sp.GetRequiredService<NamespaceDomainService>());
            services.AddSingleton<IClusterScopedWatcher<V1Namespace>,
                ClusterScopedWatcher<V1NamespaceList, V1Namespace, IKubernetesBackgroundQueue<V1Namespace>,
                    WatcherEvent<V1Namespace>>>();
        }
        else
        {
            services
                .AddSingleton<IClusterScopedResourceQueryDomainService<V1Namespace, V1NamespaceList>,
                    StaticNamespaceDomainService>();
            services.AddSingleton<IClusterScopedWatcher<V1Namespace>, StaticNamespaceWatcher>();
        }
        
        services.AddSingleton<IClusterScopedKubernetesEventCoordinator,
            ClusterScopedKubernetesEventCoordinator<IKubernetesEventDispatcher<V1Namespace>,
                IClusterScopedWatcher<V1Namespace>, V1Namespace>>();
        services.AddSingleton<IKubernetesEventDispatcher<V1Namespace>,
            KubernetesEventDispatcher<V1Namespace, IKubernetesBackgroundQueue<V1Namespace>>>();
        services.AddSingleton<IKubernetesEventProcessor<V1Namespace>, NamespaceCacheRefresher>();
        services.AddSingleton<IKubernetesEventProcessor<V1Namespace>, WatcherState<V1Namespace>>();
        services.AddSingleton<IKubernetesEventProcessor<V1Namespace>, WatcherStateAlertRefresh<V1Namespace>>();
        services.AddScoped<INamespaceService, NamespaceService>();
        
    }

    public static void AddClusterRbacAssessmentReportServices(
        this IServiceCollection services, IConfiguration kubernetesConfiguration)
    {
        bool? useServices = kubernetesConfiguration.GetValue<bool?>("TrivyUseClusterRbacAssessmentReport");
        if (useServices == null || !(bool)useServices)
        {
            services.AddScoped<IClusterRbacAssessmentReportService, ClusterRbacAssessmentReportNullService>();
            services.AddTransient<IConcurrentDictionaryCache<ClusterRbacAssessmentReportCr>, ConcurrentDictionaryCache<ClusterRbacAssessmentReportCr>>();
            return;
        }

        services
            .AddSingleton<IConcurrentDictionaryCache<ClusterRbacAssessmentReportCr>,
                ConcurrentDictionaryCache<ClusterRbacAssessmentReportCr>>();
        services
            .AddSingleton<IKubernetesBackgroundQueue<ClusterRbacAssessmentReportCr>,
                KubernetesBackgroundQueue<ClusterRbacAssessmentReportCr>>();
        services.AddSingleton<IClusterScopedWatcher<ClusterRbacAssessmentReportCr>, ClusterScopedWatcher<
            CustomResourceList<ClusterRbacAssessmentReportCr>, ClusterRbacAssessmentReportCr,
            IKubernetesBackgroundQueue<ClusterRbacAssessmentReportCr>, WatcherEvent<ClusterRbacAssessmentReportCr>>>();
        
        services.AddSingleton<IClusterScopedKubernetesEventCoordinator,
            ClusterScopedKubernetesEventCoordinator<IKubernetesEventDispatcher<ClusterRbacAssessmentReportCr>,
                IClusterScopedWatcher<ClusterRbacAssessmentReportCr>, ClusterRbacAssessmentReportCr>>();
        services.AddSingleton<IKubernetesEventDispatcher<ClusterRbacAssessmentReportCr>,
            KubernetesEventDispatcher<ClusterRbacAssessmentReportCr, IKubernetesBackgroundQueue<ClusterRbacAssessmentReportCr>>>();
        services.AddSingleton<IKubernetesEventProcessor<ClusterRbacAssessmentReportCr>,
                CacheRefresher<ClusterRbacAssessmentReportCr>>();
        services.AddSingleton<IKubernetesEventProcessor<ClusterRbacAssessmentReportCr>, WatcherState<ClusterRbacAssessmentReportCr>>();
        services.AddScoped<IClusterRbacAssessmentReportService, ClusterRbacAssessmentReportService>();
    }

    public static void AddConfigAuditReportServices(
        this IServiceCollection services, IConfiguration kubernetesConfiguration)
    {
        bool? useServices = kubernetesConfiguration.GetValue<bool?>("TrivyUseConfigAuditReport");
        if (useServices == null || !(bool)useServices)
        {
            services.AddScoped<IConfigAuditReportService, ConfigAuditReportNullService>();
            services.AddTransient<IConcurrentDictionaryCache<ConfigAuditReportCr>, ConcurrentDictionaryCache<ConfigAuditReportCr>>();
            return;
        }

        services.AddSingleton<IConcurrentDictionaryCache<ConfigAuditReportCr>,
            ConcurrentDictionaryCache<ConfigAuditReportCr>>();
        services.AddSingleton<IKubernetesBackgroundQueue<ConfigAuditReportCr>, KubernetesBackgroundQueue<ConfigAuditReportCr>>();
        services.AddSingleton<INamespacedWatcher<ConfigAuditReportCr>,
            NamespacedWatcher<CustomResourceList<ConfigAuditReportCr>, ConfigAuditReportCr,
                IKubernetesBackgroundQueue<ConfigAuditReportCr>, WatcherEvent<ConfigAuditReportCr>>>();
        
        services.AddSingleton<INamespacedKubernetesEventCoordinator,
            NamespacedKubernetesEventCoordinator<IKubernetesEventDispatcher<ConfigAuditReportCr>, 
                INamespacedWatcher<ConfigAuditReportCr>, ConfigAuditReportCr>>();
        services.AddSingleton<IKubernetesEventDispatcher<ConfigAuditReportCr>,
            KubernetesEventDispatcher<ConfigAuditReportCr, IKubernetesBackgroundQueue<ConfigAuditReportCr>>>();
        services.AddSingleton<IKubernetesEventProcessor<ConfigAuditReportCr>, CacheRefresher<ConfigAuditReportCr>>();
        services.AddSingleton<IKubernetesEventProcessor<ConfigAuditReportCr>, WatcherState<ConfigAuditReportCr>>();
        services.AddSingleton<IKubernetesEventProcessor<ConfigAuditReportCr>, WatcherStateAlertRefresh<ConfigAuditReportCr>>();
        services.AddScoped<IConfigAuditReportService, ConfigAuditReportService>();
    }

    public static void AddExposedSecretReportServices(
        this IServiceCollection services, IConfiguration kubernetesConfiguration)
    {
        bool? useServices = kubernetesConfiguration.GetValue<bool?>("TrivyUseConfigAuditReport");
        if (useServices == null || !(bool)useServices)
        {
            services.AddScoped<IExposedSecretReportService, ExposedSecretReportNullService>();
            services.AddTransient<IConcurrentDictionaryCache<ExposedSecretReportCr>, ConcurrentDictionaryCache<ExposedSecretReportCr>>();
            return;
        }

        services.AddSingleton<
            IConcurrentDictionaryCache<ExposedSecretReportCr>,
            ConcurrentDictionaryCache<ExposedSecretReportCr>>();
        services.AddSingleton<IKubernetesBackgroundQueue<ExposedSecretReportCr>, KubernetesBackgroundQueue<ExposedSecretReportCr>>();
        services.AddSingleton<INamespacedWatcher<ExposedSecretReportCr>,
            NamespacedWatcher<CustomResourceList<ExposedSecretReportCr>, ExposedSecretReportCr,
                IKubernetesBackgroundQueue<ExposedSecretReportCr>, WatcherEvent<ExposedSecretReportCr>>>();
        
        services.AddSingleton<INamespacedKubernetesEventCoordinator,
            NamespacedKubernetesEventCoordinator<IKubernetesEventDispatcher<ExposedSecretReportCr>, 
                INamespacedWatcher<ExposedSecretReportCr>, ExposedSecretReportCr>>();
        services.AddSingleton<IKubernetesEventDispatcher<ExposedSecretReportCr>,
            KubernetesEventDispatcher<ExposedSecretReportCr, IKubernetesBackgroundQueue<ExposedSecretReportCr>>>();
        services.AddSingleton<IKubernetesEventProcessor<ExposedSecretReportCr>, CacheRefresher<ExposedSecretReportCr>>();
        services.AddSingleton<IKubernetesEventProcessor<ExposedSecretReportCr>, WatcherState<ExposedSecretReportCr>>();
        services.AddSingleton<IKubernetesEventProcessor<ExposedSecretReportCr>, WatcherStateAlertRefresh<ExposedSecretReportCr>>();
        services.AddScoped<IExposedSecretReportService, ExposedSecretReportService>();
    }

    public static void AddVulnerabilityReportServices(
        this IServiceCollection services, IConfiguration kubernetesConfiguration)
    {
        bool? useServices = kubernetesConfiguration.GetValue<bool?>("TrivyUseVulnerabilityReport");
        if (useServices == null || !(bool)useServices)
        {
            services.AddScoped<IVulnerabilityReportService, VulnerabilityReportNullService>();
            services.AddTransient<IConcurrentDictionaryCache<VulnerabilityReportCr>, ConcurrentDictionaryCache<VulnerabilityReportCr>>();
            return;
        }

        services.AddSingleton<
            IConcurrentDictionaryCache<VulnerabilityReportCr>,
            ConcurrentDictionaryCache<VulnerabilityReportCr>>();
        services.AddSingleton<IKubernetesBackgroundQueue<VulnerabilityReportCr>, KubernetesBackgroundQueue<VulnerabilityReportCr>>();
        services.AddSingleton<INamespacedWatcher<VulnerabilityReportCr>,
            NamespacedWatcher<CustomResourceList<VulnerabilityReportCr>, VulnerabilityReportCr,
                IKubernetesBackgroundQueue<VulnerabilityReportCr>, WatcherEvent<VulnerabilityReportCr>>>();
        
        services.AddSingleton<INamespacedKubernetesEventCoordinator,
            NamespacedKubernetesEventCoordinator<IKubernetesEventDispatcher<VulnerabilityReportCr>,
                INamespacedWatcher<VulnerabilityReportCr>, VulnerabilityReportCr>>();
        services.AddSingleton<IKubernetesEventDispatcher<VulnerabilityReportCr>,
            KubernetesEventDispatcher<VulnerabilityReportCr, IKubernetesBackgroundQueue<VulnerabilityReportCr>>>();
        services.AddSingleton<IKubernetesEventProcessor<VulnerabilityReportCr>, CacheRefresher<VulnerabilityReportCr>>();
        services.AddSingleton<IKubernetesEventProcessor<VulnerabilityReportCr>, WatcherState<VulnerabilityReportCr>>();
        services.AddSingleton<IKubernetesEventProcessor<VulnerabilityReportCr>, WatcherStateAlertRefresh<VulnerabilityReportCr>>();
        services.AddScoped<IVulnerabilityReportService, VulnerabilityReportService>();
    }

    public static void AddClusterComplianceReportServices(
        this IServiceCollection services, IConfiguration kubernetesConfiguration)
    {
        bool? useServices = kubernetesConfiguration.GetValue<bool?>("TrivyUseClusterComplianceReport");

        if (useServices == null || !(bool)useServices)
        {
            services.AddScoped<IClusterComplianceReportService, ClusterComplianceReportNullService>();
            services.AddTransient<IConcurrentDictionaryCache<ClusterComplianceReportCr>, ConcurrentDictionaryCache<ClusterComplianceReportCr>>();
            return;
        }

        services
            .AddSingleton<IConcurrentDictionaryCache<ClusterComplianceReportCr>,
                ConcurrentDictionaryCache<ClusterComplianceReportCr>>();
        services
            .AddSingleton<IKubernetesBackgroundQueue<ClusterComplianceReportCr>, KubernetesBackgroundQueue<ClusterComplianceReportCr>>();
        services.AddSingleton<IClusterScopedWatcher<ClusterComplianceReportCr>, ClusterScopedWatcher<
            CustomResourceList<ClusterComplianceReportCr>, ClusterComplianceReportCr,
            IKubernetesBackgroundQueue<ClusterComplianceReportCr>, WatcherEvent<ClusterComplianceReportCr>>>();
        
        services.AddSingleton<IClusterScopedKubernetesEventCoordinator,
            ClusterScopedKubernetesEventCoordinator<IKubernetesEventDispatcher<ClusterComplianceReportCr>,
                IClusterScopedWatcher<ClusterComplianceReportCr>, ClusterComplianceReportCr>>();
        services.AddSingleton<IKubernetesEventDispatcher<ClusterComplianceReportCr>,
            KubernetesEventDispatcher<ClusterComplianceReportCr, IKubernetesBackgroundQueue<ClusterComplianceReportCr>>>();
        services.AddSingleton<IKubernetesEventProcessor<ClusterComplianceReportCr>, CacheRefresher<ClusterComplianceReportCr>>();
        services.AddSingleton<IKubernetesEventProcessor<ClusterComplianceReportCr>, WatcherState<ClusterComplianceReportCr>>();
        services.AddSingleton<IKubernetesEventProcessor<ClusterComplianceReportCr>, WatcherStateAlertRefresh<ClusterComplianceReportCr>>();
        services.AddScoped<IClusterComplianceReportService, ClusterComplianceReportService>();
    }

    public static void AddClusterVulnerabilityReportServices(
        this IServiceCollection services, IConfiguration kubernetesConfiguration)
    {
        bool? useServices = kubernetesConfiguration.GetValue<bool?>("TrivyUseClusterVulnerabilityReport");

        if (useServices == null || !(bool)useServices)
        {
            services.AddScoped<IClusterVulnerabilityReportService, ClusterVulnerabilityReportNullService>();
            services.AddTransient<IConcurrentDictionaryCache<ClusterVulnerabilityReportCr>, ConcurrentDictionaryCache<ClusterVulnerabilityReportCr>>();
            return;
        }

        services
            .AddSingleton<IConcurrentDictionaryCache<ClusterVulnerabilityReportCr>,
                ConcurrentDictionaryCache<ClusterVulnerabilityReportCr>>();
        services
            .AddSingleton<IKubernetesBackgroundQueue<ClusterVulnerabilityReportCr>,
                KubernetesBackgroundQueue<ClusterVulnerabilityReportCr>>();
        services.AddSingleton<IClusterScopedWatcher<ClusterVulnerabilityReportCr>, 
            ClusterScopedWatcher<CustomResourceList<ClusterVulnerabilityReportCr>, ClusterVulnerabilityReportCr,
            IKubernetesBackgroundQueue<ClusterVulnerabilityReportCr>, WatcherEvent<ClusterVulnerabilityReportCr>>>();
        
        services.AddSingleton<IClusterScopedKubernetesEventCoordinator,
            ClusterScopedKubernetesEventCoordinator<IKubernetesEventDispatcher<ClusterVulnerabilityReportCr>,
                IClusterScopedWatcher<ClusterVulnerabilityReportCr>, ClusterVulnerabilityReportCr>>();
        services.AddSingleton<IKubernetesEventDispatcher<ClusterVulnerabilityReportCr>,
            KubernetesEventDispatcher<ClusterVulnerabilityReportCr, IKubernetesBackgroundQueue<ClusterVulnerabilityReportCr>>>();
        services.AddSingleton<IKubernetesEventProcessor<ClusterVulnerabilityReportCr>, CacheRefresher<ClusterVulnerabilityReportCr>>();
        services.AddSingleton<IKubernetesEventProcessor<ClusterVulnerabilityReportCr>, WatcherState<ClusterVulnerabilityReportCr>>();
        services.AddSingleton<IKubernetesEventProcessor<ClusterVulnerabilityReportCr>, WatcherStateAlertRefresh<ClusterVulnerabilityReportCr>>();
        services.AddScoped<IClusterVulnerabilityReportService, ClusterVulnerabilityReportService>();
    }

    public static void AddClusterSbomReportServices(
        this IServiceCollection services, IConfiguration kubernetesConfiguration)
    {
        bool? useServices = kubernetesConfiguration.GetValue<bool?>("TrivyUseClusterSbomReport");

        if (useServices == null || !(bool)useServices)
        {
            services.AddScoped<IClusterSbomReportService, ClusterSbomReportNullService>();
            services.AddTransient<IConcurrentDictionaryCache<ClusterSbomReportCr>, ConcurrentDictionaryCache<ClusterSbomReportCr>>();
            return;
        }

        services
            .AddSingleton<IConcurrentDictionaryCache<ClusterSbomReportCr>,
                ConcurrentDictionaryCache<ClusterSbomReportCr>>();
        services
            .AddSingleton<IKubernetesBackgroundQueue<ClusterSbomReportCr>,
                KubernetesBackgroundQueue<ClusterSbomReportCr>>();
        services.AddSingleton<IClusterScopedWatcher<ClusterSbomReportCr>,
            ClusterScopedWatcher<CustomResourceList<ClusterSbomReportCr>, ClusterSbomReportCr,
            IKubernetesBackgroundQueue<ClusterSbomReportCr>, WatcherEvent<ClusterSbomReportCr>>>();

        services.AddSingleton<IClusterScopedKubernetesEventCoordinator,
            ClusterScopedKubernetesEventCoordinator<IKubernetesEventDispatcher<ClusterSbomReportCr>,
                IClusterScopedWatcher<ClusterSbomReportCr>, ClusterSbomReportCr>>();
        services.AddSingleton<IKubernetesEventDispatcher<ClusterSbomReportCr>,
            KubernetesEventDispatcher<ClusterSbomReportCr, IKubernetesBackgroundQueue<ClusterSbomReportCr>>>();
        services.AddSingleton<IKubernetesEventProcessor<ClusterSbomReportCr>, CacheRefresher<ClusterSbomReportCr>>();
        services.AddSingleton<IKubernetesEventProcessor<ClusterSbomReportCr>, WatcherState<ClusterSbomReportCr>>();
        services.AddSingleton<IKubernetesEventProcessor<ClusterSbomReportCr>, WatcherStateAlertRefresh<ClusterSbomReportCr>>();
        services.AddScoped<IClusterSbomReportService, ClusterSbomReportService>();
    }

    public static void AddRbacAssessmentReportServices(
        this IServiceCollection services, IConfiguration kubernetesConfiguration)
    {
        bool? useServices = kubernetesConfiguration.GetValue<bool?>("TrivyUseRbacAssessmentReport");

        if (useServices == null || !(bool)useServices)
        {
            services.AddScoped<IRbacAssessmentReportService, RbacAssessmentReportNullService>();
            services.AddTransient<IConcurrentDictionaryCache<RbacAssessmentReportCr>, ConcurrentDictionaryCache<RbacAssessmentReportCr>>();
            return;
        }

        services
            .AddSingleton<IConcurrentDictionaryCache<RbacAssessmentReportCr>,
                ConcurrentDictionaryCache<RbacAssessmentReportCr>>();
        services.AddSingleton<IKubernetesBackgroundQueue<RbacAssessmentReportCr>, KubernetesBackgroundQueue<RbacAssessmentReportCr>>();
        services.AddSingleton<INamespacedWatcher<RbacAssessmentReportCr>,
            NamespacedWatcher<CustomResourceList<RbacAssessmentReportCr>, RbacAssessmentReportCr,
                IKubernetesBackgroundQueue<RbacAssessmentReportCr>, WatcherEvent<RbacAssessmentReportCr>>>();
        
        services.AddSingleton<INamespacedKubernetesEventCoordinator,
            NamespacedKubernetesEventCoordinator<IKubernetesEventDispatcher<RbacAssessmentReportCr>,
                INamespacedWatcher<RbacAssessmentReportCr>, RbacAssessmentReportCr>>();
        services.AddSingleton<IKubernetesEventDispatcher<RbacAssessmentReportCr>,
            KubernetesEventDispatcher<RbacAssessmentReportCr, IKubernetesBackgroundQueue<RbacAssessmentReportCr>>>();
        services.AddSingleton<IKubernetesEventProcessor<RbacAssessmentReportCr>, CacheRefresher<RbacAssessmentReportCr>>();
        services.AddSingleton<IKubernetesEventProcessor<RbacAssessmentReportCr>, WatcherState<RbacAssessmentReportCr>>();
        services.AddSingleton<IKubernetesEventProcessor<RbacAssessmentReportCr>, WatcherStateAlertRefresh<RbacAssessmentReportCr>>();
        services.AddScoped<IRbacAssessmentReportService, RbacAssessmentReportService>();
    }

    public static void AddSbomReportServices(
        this IServiceCollection services, IConfiguration kubernetesConfiguration)
    {
        bool? useServices = kubernetesConfiguration.GetValue<bool?>("TrivyUseSbomReport");

        if (useServices == null || !(bool)useServices)
        {
            services.AddScoped<ISbomReportService, SbomReportNullService>();
            services.AddTransient<IConcurrentDictionaryCache<SbomReportCr>, ConcurrentDictionaryCache<SbomReportCr>>();
            return;
        }

        services
            .AddSingleton<IConcurrentDictionaryCache<SbomReportCr>,
                ConcurrentDictionaryCache<SbomReportCr>>();
        services.AddSingleton<IKubernetesBackgroundQueue<SbomReportCr>, KubernetesBackgroundQueue<SbomReportCr>>();
        services.AddSingleton<INamespacedWatcher<SbomReportCr>, SbomReportWatcher>();
        
        services.AddSingleton<INamespacedKubernetesEventCoordinator,
            NamespacedKubernetesEventCoordinator<IKubernetesEventDispatcher<SbomReportCr>,
                INamespacedWatcher<SbomReportCr>, SbomReportCr>>();
        services.AddSingleton<IKubernetesEventDispatcher<SbomReportCr>,
            KubernetesEventDispatcher<SbomReportCr, IKubernetesBackgroundQueue<SbomReportCr>>>();
        services.AddSingleton<IKubernetesEventProcessor<SbomReportCr>, CacheRefresher<SbomReportCr>>();
        services.AddSingleton<IKubernetesEventProcessor<SbomReportCr>, WatcherState<SbomReportCr>>();
        services.AddSingleton<IKubernetesEventProcessor<SbomReportCr>, WatcherStateAlertRefresh<SbomReportCr>>();
        services.AddScoped<ISbomReportService, SbomReportService>();
    }

    

    public static void AddWatcherStateServices(this IServiceCollection services)
    {
        
        services.AddSingleton<IConcurrentCache<string, WatcherStateInfo>, ConcurrentCache<string, WatcherStateInfo>>();
        //services.AddSingleton<IBackgroundQueue<WatcherStateInfo>, BackgroundQueue<WatcherStateInfo>>();
        services.AddScoped<IWatcherStatusService, WatcherStatusService>();
    }

    public static void AddAlertsServices(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddSingleton<IConcurrentCache<string, Alert>, ConcurrentCache<string, Alert>>();
        services.AddTransient<IAlertsService, AlertsService>();
    }

    public static void AddCommons(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<BackgroundQueueOptions>(configuration.GetSection("Queues"));
        services.Configure<KubernetesOptions>(configuration.GetSection("Kubernetes"));
        services.Configure<WatchersOptions>(configuration.GetSection("Watchers"));
        services.Configure<FileExportOptions>(configuration.GetSection("FileExport"));
        services.Configure<GitHubOptions>(configuration.GetSection("GitHub"));

        services.AddHostedService<KubernetesEventCoordinatorsHandlerHostedService>();
        services.AddHostedService<WatcherStateCacheTimedHostedService>();

        services.AddSingleton<IKubernetesClientFactory, KubernetesClientFactory>();
        
        if (configuration.GetSection("GitHub").GetValue<bool>("ServerCheckForUpdates"))
        {
            services.AddHttpClient<IGitHubClient, GitHubClient>(client =>
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd(Constants.UserAgentName);
            });
            services.AddHostedService<GitHubReleaseCacheTimedHostedService>();
        }
        services.AddSingleton<IConcurrentCache<long, GitHubRelease>, ConcurrentCache<long, GitHubRelease>>();
        services.AddScoped<IAppVersionService, AppVersionService>();

        services.AddHealthChecks()
            .AddCheck<WatchersLivenessHealthCheck>("watchers-liveness")
            .AddCheck<WatchersReadinessHealthCheck>("watchers-readiness");

#if DEBUG
        //services.AddHostedService<SingleBucketTimedHostedService>();
        //services.AddSingleton<IHostedService>(provider =>
        //    new MultiBucketTimedHostedService(
        //        provider.GetRequiredService<ILogger<MultiBucketTimedHostedService>>(),
        //        provider.GetRequiredService<IAlertsService>(),
        //        "MultiBucket", ["Hey!", "Yo!", "No way, Jose!"], "subLevel|mama", 3));
        //services.AddSingleton<IHostedService>(provider =>
        //    new MultiBucketTimedHostedService(
        //        provider.GetRequiredService<ILogger<MultiBucketTimedHostedService>>(),
        //        provider.GetRequiredService<IAlertsService>(),
        //        "MultiBucket", ["Hey!", "Yo!", "Yes way, Jose!"], "subLevel|dada", 3));

        services.AddScoped<IRawDomainQueryService, RawDomainQueryService>();
#endif
    }

    public static void AddUiCommons(this IServiceCollection services) =>
        services.AddScoped<IBackendSettingsService, BackendSettingsService>();

    public static void AddDomainServices(this IServiceCollection services)
    {
        services.AddSingleton<ICustomResourceDefinitionFactory, CustomResourceDefinitionFactory>();

        services
            .AddSingleton<IClusterScopedResourceWatchDomainService<ClusterComplianceReportCr,
                    CustomResourceList<ClusterComplianceReportCr>>,
                ClusterScopedTrivyReportDomainService<ClusterComplianceReportCr>>();
        services
            .AddSingleton<IClusterScopedResourceWatchDomainService<ClusterRbacAssessmentReportCr,
                    CustomResourceList<ClusterRbacAssessmentReportCr>>,
                ClusterScopedTrivyReportDomainService<ClusterRbacAssessmentReportCr>>();
        services
            .AddSingleton<IClusterScopedResourceWatchDomainService<ClusterSbomReportCr, CustomResourceList<ClusterSbomReportCr>>,
                ClusterScopedTrivyReportDomainService<ClusterSbomReportCr>>();
        services
            .AddSingleton<
                IClusterScopedResourceWatchDomainService<ClusterVulnerabilityReportCr,
                    CustomResourceList<ClusterVulnerabilityReportCr>>,
                ClusterScopedTrivyReportDomainService<ClusterVulnerabilityReportCr>>();

        services
            .AddSingleton<INamespacedResourceWatchDomainService<ConfigAuditReportCr, CustomResourceList<ConfigAuditReportCr>>,
                NamespacedTrivyReportDomainService<ConfigAuditReportCr>>();
        services
            .AddSingleton<INamespacedResourceWatchDomainService<ExposedSecretReportCr, CustomResourceList<ExposedSecretReportCr>>,
                NamespacedTrivyReportDomainService<ExposedSecretReportCr>>();
        services
            .AddSingleton<INamespacedResourceWatchDomainService<RbacAssessmentReportCr,
                    CustomResourceList<RbacAssessmentReportCr>>,
                NamespacedTrivyReportDomainService<RbacAssessmentReportCr>>();
        services
            .AddSingleton<INamespacedResourceWatchDomainService<SbomReportCr, CustomResourceList<SbomReportCr>>,
                NamespacedTrivyReportDomainService<SbomReportCr>>();
        services
            .AddSingleton<INamespacedResourceWatchDomainService<VulnerabilityReportCr, CustomResourceList<VulnerabilityReportCr>>,
                NamespacedTrivyReportDomainService<VulnerabilityReportCr>>();
    }

    public static void AddOthers(this IServiceCollection services)
    {
        services.AddScoped<ITrivyReportDependency, TrivyReportDependency>();
    }

    public static void AddOpenTelemetry(this IServiceCollection services, IConfiguration configuration, string applicationName)
    {
        if ((configuration.GetValue<bool?>("Enabled") ?? false) == false)
        {
            services.AddSingleton<IMetricsService>(provider => new MetricsService(applicationName));
            return;
        }

        string fileVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "0.0";
        string? otelEndpoint = configuration.GetValue<string>("OtelEndpoint");
        bool? isConsoleEnabled = configuration.GetValue<bool?>("ConsoleEnabled");
        bool? isAspNetCoreEnabled = configuration.GetValue<bool?>("AspNetCoreInstrumentationEnabled");
        bool? isRuntimeEnabled = configuration.GetValue<bool?>("RuntimeInstrumentationEnabled");
        int? metricsPort = configuration.GetValue<int>("PrometheusExporterPort");
        double[]? histogramBounds = configuration.GetValue<double[]>("HistogramBoundsInMs") ?? [200, 500, 1000, 5000];

        services.AddSingleton<IMetricsService>(provider => new MetricsService(applicationName));
        services.AddOpenTelemetry()
            .WithTracing(tracingBuilder =>
            {
                tracingBuilder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService(applicationName)
                        .AddAttributes(new Dictionary<string, object>
                        {
                            { "service.version", fileVersion }
                        }))
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
                        options.Protocol = (configuration.GetValue<string?>("OtelProtocol")?.ToLowerInvariant() ?? "grpc") == "grpc"
                            ? OpenTelemetry.Exporter.OtlpExportProtocol.Grpc
                            : OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                    });
                }
                if (isAspNetCoreEnabled ?? false)
                {
                    tracingBuilder.AddAspNetCoreInstrumentation(options =>
                    {
                        options.Filter = context =>
                        {
                            var path = context.Request.Path.Value;
                            return !((path?.StartsWith("/healthz") ?? false) || (path?.StartsWith("/metrics") ?? false));
                        };
                    });
                }

            })
            .WithMetrics(metricsBuilder =>
            {
                metricsBuilder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(applicationName))
                    .AddView("*", new ExplicitBucketHistogramConfiguration
                    {
                        Boundaries = histogramBounds,
                        // defaults: [ 0, 5, 10, 25, 50, 75, 100, 250, 500, 750, 1000, 2500, 5000, 7500, 10000 ]
                    })
                    .AddMeter($"{applicationName}.metrics");
                if (!string.IsNullOrWhiteSpace(otelEndpoint))
                {
                    metricsBuilder.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otelEndpoint);
                        options.Protocol = (configuration.GetValue<string?>("OtelProtocol")?.ToLowerInvariant() ?? "grpc") == "grpc"
                            ? OpenTelemetry.Exporter.OtlpExportProtocol.Grpc
                            : OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                    });
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
            });
    }
}
