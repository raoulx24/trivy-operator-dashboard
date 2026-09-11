using TrivyOperator.Dashboard.Application.GitHub.Options;
using TrivyOperator.Dashboard.Application.GitHub.Services;
using TrivyOperator.Dashboard.Application.Queries.AppVersions.Services;
using TrivyOperator.Dashboard.Application.Queries.AppVersions.Services.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.GitHub;
using TrivyOperator.Dashboard.Infrastructure.GitHub.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.GitHub.Models;

namespace TrivyOperator.Dashboard.Composition.GitHub;

public static class GitHubServiceRegistrationExtensions
{
    // 1st level - main entrance
    public static void AddGitHubRelatedServices(this IServiceCollection services, IConfiguration configuration)
    {
        GitHubCompositionMode mode = GitHubCompositionResolver.Resolve(configuration);
        
        services.Configure<GitHubOptions>(configuration.GetSection("GitHub"));

        services.AddGitHubCommonServices();

        switch (mode)
        {
            case GitHubCompositionMode.Disabled:
                return;

            case GitHubCompositionMode.Enabled:
                services.AddGitHubUpdateServices();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }
    
    // 2nd level - services registration
    private static void AddGitHubCommonServices(this IServiceCollection services)
    {
        services.AddSingleton<IConcurrentCache<long, GitHubRelease>, ConcurrentCache<long, GitHubRelease>>();

        services.AddScoped<IAppVersionsService, AppVersionsService>();
    }

    private static void AddGitHubUpdateServices(this IServiceCollection services)
    {
        services.AddHttpClient<IGitHubClient, GitHubClient>(client =>
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd(Constants.UserAgentName);
        });

        services.AddHostedService<GitHubReleaseCacheTimedHostedService>();
    }
}
