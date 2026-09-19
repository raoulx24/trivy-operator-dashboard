using k8s.Models;
using TrivyOperator.Dashboard.Application.Kubernetes.Contexts.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.WatcherRegistries.Abstractions;
using TrivyOperator.Dashboard.Application.Queries.Contexts;
using TrivyOperator.Dashboard.Application.Queries.Contexts.Abstractions;
using TrivyOperator.Dashboard.Application.Queries.Namespaces.Services;
using TrivyOperator.Dashboard.Application.Queries.Namespaces.Services.Abstractions;
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
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Contexts.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.HostedServices;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.EventDispatchers;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.EventDispatchers.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.EventPipelineStarters;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.EventPipelineStarters.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.EventProcessors;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.EventProcessors.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.EventPublishers;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.EventPublishers.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.ResourceWatches;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.ResourceWatches.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.WatcherRegistries;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.WatchSessions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.WatchSessions.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Mappers;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Mappers.Abstract;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.PersistenceAggregators;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.PersistenceAggregators.Abstracts;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Providers;
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
                services.AddNamespaceEventPipelineServices();
                break;

            case NamespaceCompositionMode.StaticMultiContext:
                CompositionLogger.Logger?.LogInformation("Adding Static Namespace related services for Multi-Context");
                services.AddNamespaceCommonServices(configuration);
                services.AddStaticNamespaceService();
                break;

            case NamespaceCompositionMode.DynamicDefaultContext:
                CompositionLogger.Logger?.LogInformation("Adding Kubernetes Namespace related services for Default Context");
                services.AddNamespaceCommonServices(configuration);
                services.AddDynamicNamespaceService();
                services.AddNamespaceEventPipelineServices();
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
    }

    private static void AddDefaultContextKubernetesServices(this IServiceCollection services)
    {
        services.AddHostedService<KubernetesEventPipelineHost>();

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
        services.AddSingleton<
            IExpiringResourceProvider<KubernetesNamespace, Uid>,
            KubernetesResourceProvider<V1Namespace, KubernetesNamespace, Uid>>();
        
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

    private static void AddNamespaceEventPipelineServices(this IServiceCollection services)
    {
        // event pipeline starter
        services.AddSingleton<IKubernetesEventPipelineStarter, ClusterScopedEventPipelineStarter<V1Namespace>>();
        
        // kubernetes resource watcher
        services.AddSingleton<
            IKubernetesResourceWatch<V1NamespaceList, V1Namespace>,
            ClusterScopedResourceWatch<V1NamespaceList, V1Namespace>>();

        // kubernetes event publisher
        services.AddSingleton<IKubernetesEventPublisher<V1Namespace>,KubernetesEventPublisher<V1Namespace>>();

        // kubernetes watch session factory
        services.AddSingleton<
            IKubernetesWatchSessionFactory<V1NamespaceList, V1Namespace>,
            KubernetesWatchSessionFactory<V1NamespaceList, V1Namespace>>();

        // kubernetes watcher registry
        services.AddSingleton<
            IClusterScopedWatcherRegistry,
            ClusterScopedWatcherRegistry<V1NamespaceList, V1Namespace>>();

        // // watcher
        // services.AddSingleton<IClusterScopedWatcher, ClusterScopedWatcher<V1NamespaceList, V1Namespace>>();

        // background queue
        services.AddSingleton<IKubernetesBackgroundQueue<V1Namespace>, KubernetesBackgroundQueue<V1Namespace>>();

        // event dispatcher
        services.AddSingleton<
            IKubernetesEventDispatcher<V1Namespace>,
            KubernetesEventDispatcher<V1Namespace, IKubernetesBackgroundQueue<V1Namespace>>>();

        // processor for starting namespaced watchers
        services.AddSingleton<IKubernetesEventProcessor<V1Namespace>, NamespacedWatcherLifecycleProcessor>();
    }
}
