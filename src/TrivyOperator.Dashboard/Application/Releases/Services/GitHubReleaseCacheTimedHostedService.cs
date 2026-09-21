using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.Queries.AppVersions.Services;
using TrivyOperator.Dashboard.Application.Releases.Abstractions;
using TrivyOperator.Dashboard.Application.Releases.Options;
using TrivyOperator.Dashboard.Application.Shared.Abstractions;
using TrivyOperator.Dashboard.Domain.Releases.Entities;
using TrivyOperator.Dashboard.Domain.Releases.ValueObjects;

namespace TrivyOperator.Dashboard.Application.Releases.Services;

public sealed class GitHubReleaseCacheTimedHostedService(
    IReleaseProvider gitHubClient,
    IOptions<ReleaseOptions> options,
    ICache<ReleaseId, Release> cache,
    ILogger<AppVersionsService> logger
) : IHostedService, IDisposable
{
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
            logger.LogInformation("GitHub Release Cache Timed Hosted Service stopped.");
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
        
        logger.LogInformation("GitHub Release Cache Timed Hosted Service stopped.");
    }

    ~GitHubReleaseCacheTimedHostedService()
    {
        Dispose(false);
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
        if (!options.Value.ServerCheckForUpdates)
            return;
        
        try
        {
            Release[]? releases = await gitHubClient.GitHubReleases(
                options.Value.BaseTrivyDashboardRepoUrl,
                cancellationToken
            );
            if (releases is null)
            {
                logger.LogWarning("Failed to fetch releases");
                return;
            }

            Release? latestRelease = await gitHubClient.GetLatestRelease(
                options.Value.BaseTrivyDashboardRepoUrl,
                cancellationToken
            );
            
            if (latestRelease != null)
            {
                releases =
                [
                    .. releases.Select(release =>
                        release.Id == latestRelease.Id ? release.MarkAsLatest() : release.MarkAsNotLatest()
                    ),
                ];
            }

            cache.Clear();
            foreach (Release release in releases)
            {
                cache.TryAdd(release.Id, release);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error occurred while executing the timed hosted service - {exceptionMessage}",
                ex.Message
            );
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
