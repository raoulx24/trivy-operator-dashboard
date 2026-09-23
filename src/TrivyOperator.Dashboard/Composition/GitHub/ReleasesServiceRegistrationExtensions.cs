using TrivyOperator.Dashboard.Application.Queries.AppVersions.Services;
using TrivyOperator.Dashboard.Application.Queries.AppVersions.Services.Abstractions;
using TrivyOperator.Dashboard.Application.Releases.Abstractions;
using TrivyOperator.Dashboard.Application.Releases.Options;
using TrivyOperator.Dashboard.Application.Releases.Services;
using TrivyOperator.Dashboard.Application.Shared.Cache.Abstractions;
using TrivyOperator.Dashboard.Composition.Shared;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.GitHub;
using TrivyOperator.Dashboard.Infrastructure.GitHub.Models;
using TrivyOperator.Dashboard.Infrastructure.GitHub.Services;

namespace TrivyOperator.Dashboard.Composition.GitHub;

public static class ReleasesServiceRegistrationExtensions
{
    // 1st level - main entrance
    public static void AddGitHubRelatedServices(this IServiceCollection services, IConfiguration configuration)
    {
        GitHubCompositionMode mode = ReleasesCompositionResolver.Resolve(configuration);
        
        services.Configure<ReleaseOptions>(configuration.GetSection("GitHub"));

        services.AddGitHubCommonServices();

        switch (mode)
        {
            case GitHubCompositionMode.Disabled:
                CompositionLogger.Logger?.LogInformation("GitHub related services are disabled");
                return;

            case GitHubCompositionMode.Enabled:
                CompositionLogger.Logger?.LogInformation("Adding GitHub related services");
                services.AddGitHubUpdateServices();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }
    
    // 2nd level - services registration
    private static void AddGitHubCommonServices(this IServiceCollection services)
    {
        services.AddSingleton<ICache<long, GitHubRelease>, Cache<long, GitHubRelease>>();

        services.AddScoped<IAppVersionsService, AppVersionsService>();
    }

    private static void AddGitHubUpdateServices(this IServiceCollection services)
    {
        services.AddHttpClient<IReleaseProvider, GitHubClient>(client =>
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd(Constants.UserAgentName);
        });

        services.AddHostedService<GitHubReleaseCacheTimedHostedService>();
    }
}
