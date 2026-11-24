using k8s.Models;
using TrivyOperator.Dashboard.Application.Kubernetes.Contexts.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventDispatchers;
using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventDispatchers.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventPipelineStarters;
using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventProcessors;
using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventProcessors.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.Namespaces;
using TrivyOperator.Dashboard.Application.Kubernetes.Namespaces.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.Queries.Contexts;
using TrivyOperator.Dashboard.Application.Kubernetes.Queries.Contexts.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.Queries.Namespaces.Services;
using TrivyOperator.Dashboard.Application.Kubernetes.Queries.Namespaces.Services.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.WatcherRegistries.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.EventPipeline.EventPipelineStarters.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.EventPipeline.HostedServices;
using TrivyOperator.Dashboard.Composition.Shared;
using TrivyOperator.Dashboard.Domain.Kubernetes.Entities;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.BackgroundQueues;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.CacheEntries;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.CacheEntryBuilders;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.CacheEntryBuilders.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.ClientFactory;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.ClientFactory.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Contexts;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.BackgroundQueues;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.EventPublishers;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.EventPublishers.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.ResourceWatches;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.ResourceWatches.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.WatcherRegistries;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.WatchSessions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.WatchSessions.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.WatchSessions.Failures;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.WatchSessions.Failures.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Mappers;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Mappers.Abstract;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.PersistenceAggregators;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.PersistenceAggregators.Abstracts;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Providers;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.ResourceMaterializer;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.ResourceMaterializer.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Services;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Services.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.StaticResources.Services;

namespace TrivyOperator.Dashboard.Composition.Kubernetes;

public static class KubernetesServiceRegistrationExtensions
{
    // 1st level - main entrances
    public static void AddKubernetesRelatedServices(this IServiceCollection services, IConfiguration configuration)
    {
        KubernetesCompositionMode mode = KubernetesCompositionResolver.Resolve(configuration);

        switch (mode)
        {
            case KubernetesCompositionMode.Disabled:
                CompositionLogger.Logger?.LogInformation("Kubernetes related services are disabled");
                // the contexts endpoint is still called by the frontend - no contexts
                services.AddScoped<IKubernetesContextService, KubernetesContextNullService>();
                return;

            case KubernetesCompositionMode.DefaultContext:
                CompositionLogger.Logger?.LogInformation("Adding Kubernetes related services for Default Context");
                services.AddKubernetesClientServices();
                services.AddDefaultContextKubernetesServices();
                break;

            case KubernetesCompositionMode.MultiContext:
                CompositionLogger.Logger?.LogInformation("Adding Kubernetes related services for Multi-Context");
                services.AddKubernetesClientServices();
                services.AddMultiContextKubernetesServices();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }

    public static void AddNamespaceRelatedServices(this IServiceCollection services, IConfiguration configuration)
    {
        NamespaceCompositionMode mode = NamespaceCompositionResolver.Resolve(configuration);

        switch (mode)
        {
            case NamespaceCompositionMode.Disabled:
                CompositionLogger.Logger?.LogInformation("Kubernetes Namespace related services are disabled");
                services.AddSingleton<IKubernetesNamespaceService, KubernetesNamespaceNullService>();
                break;

            case NamespaceCompositionMode.StaticDefaultContext:
                CompositionLogger.Logger?.LogInformation("Adding Static Namespace related services for Default Context");
                services.AddNamespaceCommonServices(configuration);
                services.AddStaticNamespaceService();
                break;

            case NamespaceCompositionMode.StaticMultiContext:
                CompositionLogger.Logger?.LogInformation("Adding Static Namespace related services for Multi-Context");
                services.AddNamespaceCommonServices(configuration);
                services.AddStaticNamespaceService();
                break;

            case NamespaceCompositionMode.DynamicDefaultContext:
                CompositionLogger.Logger?.LogInformation("Adding Kubernetes Namespace related services for Default Context");
                // the report watchers are cluster wide, so no namespace list is kept; the namespaces endpoint answers empty
                // the namespace service stays, as the namespaced report services depend on it (used by multi-context)
                // the queue options are configured here too, as AddNamespaceCommonServices (which also does it) is not called
                services.Configure<BackgroundQueueOptions>(configuration.GetSection("Queues"));
                services.AddDynamicNamespaceService();
                services.AddSingleton<IKubernetesNamespaceService, KubernetesNamespaceNullService>();
                break;

            case NamespaceCompositionMode.DynamicMultiContext:
                CompositionLogger.Logger?.LogInformation("Adding Kubernetes Namespace related services for Multi-Context");
                services.AddNamespaceCommonServices(configuration);
                services.AddDynamicNamespaceService();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }
    
    // 2nd level - adding of services
    
    // -- kubernetes related services
    
    private static void AddKubernetesClientServices(this IServiceCollection services)
    {
        // KubernetesOptions is registered by the BackendSettings composition root
        
        services.AddSingleton<KubernetesClientFactory>();
        services.AddSingleton<IKubernetesClientFactory>(sp => sp.GetRequiredService<KubernetesClientFactory>());
        services.AddSingleton<IContextProvider>(sp => sp.GetRequiredService<KubernetesClientFactory>());

        services.AddScoped<IKubernetesContextService, KubernetesContextService>();

        // one throttle shared by all watch sessions (all types, all contexts)
        services.AddSingleton<IKubernetesWatchSessionThrottle, KubernetesWatchSessionThrottle>();

        // stateless; turns watch session exceptions into failure categories
        services.AddSingleton<IWatchFailureClassifier, WatchFailureClassifier>();

        // which namespaces the main pipeline accepts (NamespaceList / ExcludedNamespaceList / reserved); fixed at startup
        services.AddSingleton<INamespacePolicyProvider, NamespacePolicyProvider>();
    }

    private static void AddDefaultContextKubernetesServices(this IServiceCollection services)
    {
        services.AddHostedService<EventPipelineHostedService>();

        // starts the namespaced report watchers (cluster wide or per namespace) once all pipelines are started
        services.AddSingleton<IWatchersStarter, NamespacedWatchersStarter>();

        services.AddSingleton<IKubernetesContextResolver, DefaultKubernetesContextResolver>();
    }

    private static void AddMultiContextKubernetesServices(this IServiceCollection services)
    {
        services.AddSingleton<IKubernetesContextResolver, HttpHeaderKubernetesContextResolver>();
    }

    // -- namespace related services

    private static void AddNamespaceCommonServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<BackgroundQueueOptions>(configuration.GetSection("Queues"));
        
        // resource mapper
        services.AddSingleton<KubernetesNamespaceMapper>();

        services.AddSingleton<
            IResourceMapper<V1Namespace, KubernetesNamespace>>(sp => sp.GetRequiredService<KubernetesNamespaceMapper>());

        services.AddSingleton<
            IResourceKeyProvider<V1Namespace, Uid>>(sp => sp.GetRequiredService<KubernetesNamespaceMapper>());

        // cache entry builder
        services.AddSingleton<ICacheEntryBuilder<KubernetesNamespace, Uid>, KubernetesNamespaceCacheEntryBuilder>();
        services.AddSingleton<ICacheEntryBuilder<KubernetesNamespace, KubernetesNamespace, Uid>>(sp =>
            sp.GetRequiredService<ICacheEntryBuilder<KubernetesNamespace, Uid>>());
        
        // ICacheEntityCodec is registered by the Trivy composition root

        // expiring cache
        services.AddSingleton<
            IExpiringResourceDictionaryCache<Uid, CacheEntry<KubernetesNamespace, Uid>>,
            ExpiringResourceDictionaryCache<Uid, CacheEntry<KubernetesNamespace, Uid>>>();

        // aggregator
        services.AddSingleton<
            IResourceAggregator<V1Namespace, KubernetesNamespace, Uid>,
            UidKeyedResourceAggregator<V1Namespace, KubernetesNamespace>>();

        // resource provider
        services.AddSingleton<KubernetesResourceProvider<V1Namespace, KubernetesNamespace, KubernetesNamespace, Uid>>();
        services.AddSingleton<
            IExpiringResourceProvider<KubernetesNamespace, Uid>>(
            sp => sp.GetRequiredService<KubernetesResourceProvider<V1Namespace, KubernetesNamespace, KubernetesNamespace, Uid>>());
        services.AddSingleton<
            IResourceProvider<KubernetesNamespace, Uid>>(
            sp => sp.GetRequiredService<KubernetesResourceProvider<V1Namespace, KubernetesNamespace, KubernetesNamespace, Uid>>());
        
        // query service
        services.AddScoped<IKubernetesNamespaceService, KubernetesNamespaceService>();
    }

    private static void AddStaticNamespaceService(this IServiceCollection services)
    {
        services.AddSingleton<StaticNamespaceService>();
        services.AddSingleton<
            IClusterScopedResourceService<V1Namespace, V1NamespaceList>>(
            sp => sp.GetRequiredService<StaticNamespaceService>());

        services.AddSingleton<
            IKubernetesResourceService<V1Namespace>>(
            sp => sp.GetRequiredService<StaticNamespaceService>());
    }

    private static void AddDynamicNamespaceService(this IServiceCollection services)
    {
        services.AddSingleton<NamespaceService>();

        services.AddSingleton<
            IClusterScopedResourceService<V1Namespace, V1NamespaceList>>(
            sp => sp.GetRequiredService<NamespaceService>());

        services.AddSingleton<
            IKubernetesResourceService<V1Namespace>>(
            sp => sp.GetRequiredService<NamespaceService>());
    }
}
