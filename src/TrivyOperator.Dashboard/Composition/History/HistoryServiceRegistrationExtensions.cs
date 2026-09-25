using TrivyOperator.Dashboard.Application.History.EventPipeline.BackgroundQueue;
using TrivyOperator.Dashboard.Application.History.EventPipeline.EventDispatchers;
using TrivyOperator.Dashboard.Application.History.EventPipeline.EventDispatchers.Abstractions;
using TrivyOperator.Dashboard.Application.History.EventPipeline.EventProcessors.Abstractions;
using TrivyOperator.Dashboard.Application.History.EventPipeline.EventPublisher;
using TrivyOperator.Dashboard.Application.History.EventPipeline.EventPublisher.Abstractions;
using TrivyOperator.Dashboard.Application.History.VulnerabilityReportsHistory.EventProcessors;
using TrivyOperator.Dashboard.Application.History.VulnerabilityReportsHistory.KubernetesEventProcessors;
using TrivyOperator.Dashboard.Application.History.VulnerabilityReportsHistory.Retention;
using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventProcessors.Abstractions;
using TrivyOperator.Dashboard.Application.Queries.History.Services;
using TrivyOperator.Dashboard.Application.Queries.History.Services.Abstractions;
using TrivyOperator.Dashboard.Composition.Shared;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.Entities;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.Services;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.Services.Abstractions;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Infrastructure.Caching.CacheEntityCodec.Factories;
using TrivyOperator.Dashboard.Infrastructure.Caching.CacheEntityCodec.Factories.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.Distributed;
using TrivyOperator.Dashboard.Infrastructure.Caching.Distributed.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.History.EventPipeline.BackgroundQueues;
using TrivyOperator.Dashboard.Infrastructure.History.Migrations.Migrator;
using TrivyOperator.Dashboard.Infrastructure.History.Migrations.Migrator.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.History.Stores;

namespace TrivyOperator.Dashboard.Composition.History;

public static class HistoryServiceRegistrationExtensions
{
    // 1st level - main entrances
    public static void AddHistoryRelatedServices(this IServiceCollection services, IConfiguration configuration)
    { 
        HistoryCompositionMode mode = HistoryCompositionResolver.Resolve(configuration);

        switch (mode)
        {
            case HistoryCompositionMode.Disabled:
                CompositionLogger.Logger?.LogInformation("History related services are disabled");
                services.AddTransient<
                    IVulnerabilityReportsHistoryService,
                    VulnerabilityReportsHistoryNullService>();

                services.AddSingleton<
                    IVulnerabilityReportsHistoryStore,
                    DistributedCacheVulnerabilityReportsHistoryNullStore>();
                
                services.AddSingleton<IPersistenceMigrationRunner, PersistenceMigrationNullRunner>();

                break;

            case HistoryCompositionMode.DistributedCache:
                CompositionLogger.Logger?.LogInformation("Adding History related services");
                services.AddDistributedCacheHistoryServices(configuration);
                services.AddHistoryMigrationServices();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
        
        // TODO: add this in program.cs
        // builder.Services.AddHistoryRelatedServices(builder.Configuration);
        //
        // WebApplication app = builder.Build();
        //
        // await app.RunHistoryMigrationsAsync();
    }
    
    // 2nd level - services registration
    private static void AddDistributedCacheHistoryServices(this IServiceCollection services, IConfiguration configuration)
    {
        // registered in background settings. left here for reference
        // services.Configure<VulnerabilityReportsHistoryOptions>(configuration.GetSection("History"));
        // services.Configure<RetentionOptions>(configuration.GetSection("History").GetSection("Retention"));
        
        services.Configure<DistributedCacheClientOptions>(configuration
                .GetSection("History")
                .GetSection("DistributedCache"));

        services.Configure<DistributedCacheClientOptions>(
            configuration
                .GetSection("History")
                .GetSection("DistributedCache")
                .GetSection("RetryOptions"));

        services.AddSingleton<DistributedCacheConnectionProvider>();
        services.AddHostedService<DistributedCacheConnectionProvider>();

        services.AddSingleton<IDistributedCacheClientFactory, DistributedCacheClientFactory>();

        services.AddSingleton<ICacheEntityCodecFactory, CacheEntityCodecFactory>();

        services.AddSingleton<IDistributedCacheExecutor, DistributedCacheExecutor>();

        services.AddSingleton<IVulnerabilityReportsHistoryStore, DistributedCacheVulnerabilityReportsHistoryStore>();

        services.AddScoped<IVulnerabilityReportsHistoryRetentionService, VulnerabilityReportsHistoryRetentionService>();

        services.AddSingleton<IKubernetesEventProcessor<VulnerabilityReport, Digest>, VulnerabilityReportsHistoryRefresher>();

        services.AddScoped<IVulnerabilityReportsHistoryService, VulnerabilityReportsHistoryService>();

        services.AddHostedService<VulnerabilityReportsHistoryRetentionTimedHostedService>();
        
        services.AddHistoryEventPipelineServices<Snapshot>();
    }

    private static void AddHistoryEventPipelineServices<THistoryResource>(this IServiceCollection services)
        where THistoryResource : class
    {
        services.AddSingleton<IHistoryEventPublisher<THistoryResource>, HistoryEventPublisher<THistoryResource>>();
        
        services.AddSingleton<IHistoryBackgroundQueue<THistoryResource>, HistoryBackgroundQueue<THistoryResource>>();

        services.AddSingleton<IHistoryEventDispatcher<THistoryResource>, HistoryEventDispatcher<THistoryResource>>();
        
        services.AddSingleton<IHistoryEventProcessor<Snapshot>, VulnerabilityReportsHistoryMetricsProcessor> ();
    }
}
