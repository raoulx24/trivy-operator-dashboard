using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.AppVersions.Services;
using TrivyOperator.Dashboard.Application.AppVersions.Services.Options;
using TrivyOperator.Dashboard.Infrastructure.Caching.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Clients.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Clients.Models;

namespace TrivyOperator.Dashboard.Infrastructure.Clients;

public sealed class GitHubReleaseCacheTimedHostedService(
    IGitHubClient gitHubClient, 
    IOptions<GitHubOptions> options,
    IConcurrentCache<long, GitHubRelease> cache,
    ILogger<AppVersionService> logger)
    : IHostedService, IDisposable
{
    ~GitHubReleaseCacheTimedHostedService()
    {
        Dispose(false);
    }
    
    private readonly int timeFrameInMinutes = options.Value.CheckForUpdatesIntervalInMinutes;
    private bool disposed;
    private Task? executingTask;
    private CancellationTokenSource? stoppingCts;
    private Timer? timer;
    
    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("GitHub Release Cache Timed Hosted Service is starting.");

        stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timer = new Timer(Execute, null, TimeSpan.Zero, TimeSpan.FromMinutes(timeFrameInMinutes));

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("GitHub Release Cache Timed Hosted Service is stopping.");

        timer?.Change(Timeout.Infinite, 0);

        if (executingTask == null || executingTask.IsCompleted)
        {
            return;
        }

        try
        {
            await stoppingCts!.CancelAsync();
        }
        finally
        {
            await executingTask.WaitAsync(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
        }
    }

    private void Execute(object? state)
    {
        if (executingTask?.IsCompleted ?? true)
        {
            executingTask = ExecuteAsync(stoppingCts?.Token ?? CancellationToken.None);
        }
        else
        {
            logger.LogWarning(
                "GitHub Release Cache Timed Hosted Service is still running previous execution, wait for next cycle."
            );
        }
    }

    private async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            GitHubRelease[]? releases = await gitHubClient.GitHubReleases(options.Value.BaseTrivyDashboardRepoUrl, cancellationToken);
            if (releases is null)
            {
                logger.LogWarning("Failed to fetch releases from GitHub.");
                return;
            }
            GitHubRelease? latestRelease = await gitHubClient.GetLatestRelease(options.Value.BaseTrivyDashboardRepoUrl, cancellationToken);
            if (latestRelease != null)
            {
                GitHubRelease? release = releases.FirstOrDefault(x => x.Id == latestRelease.Id);
                if (release != null)
                {
                    release.IsLatest = true;
                }
            }
            cache.Clear();
            foreach (GitHubRelease release in releases)
            {
                cache.TryAdd(release.Id, release);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while executing the timed hosted service - {exceptionMessage}", ex.Message);
        }
    }

    private void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        if (disposing)
        {
            timer?.Dispose();
            stoppingCts?.Cancel();
        }

        disposed = true;
    }
}
