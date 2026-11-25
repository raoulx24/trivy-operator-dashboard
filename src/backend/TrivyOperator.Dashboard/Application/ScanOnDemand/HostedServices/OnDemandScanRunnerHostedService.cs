using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.ScanOnDemand.Abstractions;
using TrivyOperator.Dashboard.Application.ScanOnDemand.Models;
using TrivyOperator.Dashboard.Application.ScanOnDemand.Options;
using TrivyOperator.Dashboard.Application.Shared.Jobs.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.Jobs.Models;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.ScanOnDemand.Entities;
using TrivyOperator.Dashboard.Domain.ScanOnDemand.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.ScanOnDemand.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Application.ScanOnDemand.HostedServices;

// drives the scans; the store is the source of truth, so a restart simply continues
// one loop, every PollIntervalSeconds or when a submit wakes it up:
// - Running scans: pod gone or failed -> Failed; read the requested types not collected yet; the first report starts the
//   grace period; done when all arrived or the grace period is over -> save the result, Completed, delete the pod;
//   nothing after ScanTimeoutSeconds -> TimedOut, delete the pod
// - Pending scans, while fewer than MaxConcurrentScans run: create the pod -> Running; refused -> Failed; other errors
//   are retried, until the scan is older than the timeout
// - at startup and every 10 minutes: delete the scan pods whose scan is not running (crashes, failed deletes)
// single replica only: two instances would process the same scans
public sealed class OnDemandScanRunnerHostedService(
    IOnDemandScanStore store,
    IScanPodClient podClient,
    IScanReportReader reportReader,
    IOnDemandScanWakeUp wakeUp,
    IScanOnDemandAvailability availability,
    IOptions<ScanOnDemandOptions> options,
    IJobStatusRegistry jobs,
    ILogger<OnDemandScanRunnerHostedService> logger
) : IHostedService, IDisposable
{
    private const string CleanupJobName = "Scan on demand cleanup";
    private const string PodNamePrefix = "sod-";
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromMinutes(10);

    // the reports collected so far, per running scan; touched by the loop only
    private readonly Dictionary<ScanId, CollectedReports> collected = [];

    private CancellationTokenSource? stoppingCts;
    private Task? executingTask;
    private DateTime nextCleanupAt = DateTime.MinValue;
    private bool disposed;

    private ScanOnDemandOptions Options => options.Value;

    public Task StartAsync(CancellationToken ctx)
    {
        // the startup checks ran before the hosted services start
        if (!availability.IsAvailable)
        {
            logger.LogInformation("Scan on demand runner is not started: {reason}", availability.Reason);
            return Task.CompletedTask;
        }

        logger.LogInformation("Scan on demand runner is starting.");

        stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(ctx);
        jobs.Register(new JobDefinition(CleanupJobName, CleanupJobName, CleanupInterval));
        executingTask = Task.Run(() => RunAsync(stoppingCts.Token), CancellationToken.None);

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken ctx)
    {
        logger.LogInformation("Scan on demand runner is stopping.");

        if (executingTask is null || executingTask.IsCompleted)
        {
            return;
        }

        try
        {
            await stoppingCts!.CancelAsync();
        }
        finally
        {
            await executingTask.WaitAsync(ctx).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
        }

        logger.LogInformation("Scan on demand runner stopped.");
    }

    private async Task RunAsync(CancellationToken ctx)
    {
        TimeSpan pollInterval = TimeSpan.FromSeconds(Math.Max(1, Options.PollIntervalSeconds));

        while (!ctx.IsCancellationRequested)
        {
            try
            {
                await TickAsync(ctx);
            }
            catch (OperationCanceledException) when (ctx.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Scan on demand runner tick failed; retrying in {pollInterval}", pollInterval);
            }

            try
            {
                await wakeUp.WaitAsync(pollInterval, ctx);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task TickAsync(CancellationToken ctx)
    {
        if (DateTime.UtcNow >= nextCleanupAt)
        {
            nextCleanupAt = DateTime.UtcNow.Add(CleanupInterval);
            await CleanupAsync(ctx);
        }

        OnDemandScan[] active =
        [
            .. (await store.GetScansAsync(ctx))
                .Where(static x => !x.IsFinal)
                .OrderBy(static x => x.CreatedAt),
        ];

        // forget the collected reports of scans that are no longer running (deleted, finished elsewhere)
        foreach (ScanId id in collected.Keys.Where(id => active.All(x => x.Id != id || x.State != ScanState.Running)).ToArray())
        {
            collected.Remove(id);
        }

        int running = 0;

        foreach (OnDemandScan scan in active.Where(static x => x.State == ScanState.Running))
        {
            if (await TryStepAsync(scan, ProcessRunningAsync, ctx))
            {
                running++;
            }
        }

        foreach (OnDemandScan scan in active.Where(static x => x.State == ScanState.Pending))
        {
            if (running >= Options.MaxConcurrentScans)
            {
                break;
            }

            if (await TryStepAsync(scan, StartScanAsync, ctx))
            {
                running++;
            }
        }
    }

    // true when the scan is (still) running after the step; an error keeps it as it was, for the next tick
    private async Task<bool> TryStepAsync(
        OnDemandScan scan,
        Func<OnDemandScan, CancellationToken, Task<bool>> step,
        CancellationToken ctx)
    {
        try
        {
            return await step(scan, ctx);
        }
        catch (OperationCanceledException) when (ctx.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Scan {scanId} could not be processed; retrying next time", scan.Id);
            return scan.State == ScanState.Running;
        }
    }

    private async Task<bool> StartScanAsync(OnDemandScan scan, CancellationToken ctx)
    {
        Timestamp now = Timestamp.Now();
        ResourceName podName;

        try
        {
            podName = await podClient.CreatePodAsync(scan.Id, scan.Image, ctx);
        }
        catch (ScanPodRejectedException ex)
        {
            await FinishAsync(scan.MarkFailed(ex.Message, now), deletePod: false, ctx);
            return false;
        }
        catch (Exception ex) when (ex is not OperationCanceledException
                                   && now.Value - scan.CreatedAt.Value > ScanTimeout)
        {
            await FinishAsync(scan.MarkFailed($"The scan pod could not be created: {ex.Message}", now), deletePod: false, ctx);
            return false;
        }

        await store.SaveScanAsync(scan.MarkRunning(podName, now), ctx);
        logger.LogInformation("Scan {scanId} started with pod {podName}", scan.Id, podName);

        return true;
    }

    private async Task<bool> ProcessRunningAsync(OnDemandScan scan, CancellationToken ctx)
    {
        Timestamp now = Timestamp.Now();
        ScanPodStatus? status = await podClient.GetPodStatusAsync(scan.PodName, ctx);

        if (status is null)
        {
            await FinishAsync(scan.MarkFailed("The scan pod no longer exists.", now), deletePod: false, ctx);
            return false;
        }

        if (status.IsFailed)
        {
            string message = string.Join(": ", new[] { status.Reason, status.Message, }.Where(x => !string.IsNullOrWhiteSpace(x)));
            await FinishAsync(scan.MarkFailed(message.Length > 0 ? message : $"The scan pod failed ({status.Phase}).", now), true, ctx);
            return false;
        }

        if (!collected.TryGetValue(scan.Id, out CollectedReports? reports))
        {
            reports = new CollectedReports();
            collected[scan.Id] = reports;
        }

        HashSet<ScanReportType> missing = [.. scan.RequestedReportTypes.Where(x => !reports.Present.Contains(x)),];

        if (missing.Count > 0)
        {
            reports.Merge(await reportReader.ReadAsync(scan.PodName, missing, ctx), now, scan.Id, logger);
        }

        if (reports.FirstReportAt is { } firstReportAt)
        {
            bool allIn = reports.Present.IsSupersetOf(scan.RequestedReportTypes);

            if (allIn || now.Value - firstReportAt.Value >= GracePeriod)
            {
                await CompleteAsync(scan, reports, now, ctx);
                return false;
            }

            return true;
        }

        Timestamp startedAt = scan.StartedAt ?? scan.UpdatedAt;

        if (now.Value - startedAt.Value > ScanTimeout)
        {
            await FinishAsync(scan.MarkTimedOut(now), deletePod: true, ctx);
            return false;
        }

        return true;
    }

    private async Task CompleteAsync(OnDemandScan scan, CollectedReports reports, Timestamp now, CancellationToken ctx)
    {
        OnDemandScanResult result = new(
            reports.Digest,
            scan.Id,
            now,
            new Timestamp(now.Value.AddHours(Options.ResultTtlHours)),
            reports.VulnerabilityReport,
            reports.ExposedSecretReport,
            reports.SbomReport);

        await store.SaveResultAsync(result, ctx);
        await FinishAsync(scan.MarkCompleted(reports.Digest, reports.Present, now), deletePod: true, ctx);
    }

    // the scan is saved first: a pod that is not deleted here is removed by the cleanup
    private async Task FinishAsync(OnDemandScan scan, bool deletePod, CancellationToken ctx)
    {
        await store.SaveScanAsync(scan, ctx);
        collected.Remove(scan.Id);

        logger.LogInformation(
            "Scan {scanId} of {image} ended: {state}{failure}",
            scan.Id,
            scan.Image.FullName,
            scan.State,
            scan.FailureMessage is null ? string.Empty : $" - {scan.FailureMessage}");

        if (!deletePod || !scan.PodName.IsValid)
        {
            return;
        }

        try
        {
            await podClient.DeletePodAsync(scan.PodName, ctx);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Scan pod {podName} could not be deleted; the cleanup will retry", scan.PodName);
        }
    }

    private async Task CleanupAsync(CancellationToken ctx)
    {
        jobs.Started(CleanupJobName);

        try
        {
            HashSet<string> runningPods =
            [
                .. (await store.GetScansAsync(ctx))
                    .Where(static x => x.State == ScanState.Running && x.PodName.IsValid)
                    .Select(static x => x.PodName.Value),
            ];

            int deleted = 0;

            foreach (ResourceName pod in await podClient.ListScanPodsAsync(ctx))
            {
                if (!pod.Value.StartsWith(PodNamePrefix, StringComparison.Ordinal) || runningPods.Contains(pod.Value))
                {
                    continue;
                }

                await podClient.DeletePodAsync(pod, ctx);
                deleted++;
                logger.LogInformation("Deleted leftover scan pod {podName}", pod);
            }

            jobs.Succeeded(CleanupJobName, $"{deleted} leftover scan pods deleted");
        }
        catch (OperationCanceledException) when (ctx.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Scan on demand cleanup failed");
            jobs.Failed(CleanupJobName, ex.Message);
        }
    }

    private TimeSpan ScanTimeout => TimeSpan.FromSeconds(Options.ScanTimeoutSeconds);

    private TimeSpan GracePeriod => TimeSpan.FromSeconds(Options.ReportsGracePeriodSeconds);

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        stoppingCts?.Dispose();
    }

    private sealed class CollectedReports
    {
        public Digest Digest { get; private set; } = new(null);
        public Timestamp? FirstReportAt { get; private set; }
        public VulnerabilityReport? VulnerabilityReport { get; private set; }
        public ExposedSecretReport? ExposedSecretReport { get; private set; }
        public SbomReport? SbomReport { get; private set; }
        public HashSet<ScanReportType> Present { get; } = [];

        // a report of another digest than the ones already collected is ignored (not expected: one pod, one image)
        public void Merge(ScanReports reports, Timestamp now, ScanId scanId, ILogger logger)
        {
            if (!reports.ImageDigest.IsValid)
            {
                return;
            }

            if (Digest.IsValid && Digest != reports.ImageDigest)
            {
                logger.LogError(
                    "Scan {scanId}: reports for digest {digest} ignored; the scan already has reports for {existingDigest}",
                    scanId,
                    reports.ImageDigest,
                    Digest);
                return;
            }

            Digest = reports.ImageDigest;
            FirstReportAt ??= now;

            if (reports.VulnerabilityReport is not null)
            {
                VulnerabilityReport = reports.VulnerabilityReport;
                Present.Add(ScanReportType.VulnerabilityReport);
            }

            if (reports.ExposedSecretReport is not null)
            {
                ExposedSecretReport = reports.ExposedSecretReport;
                Present.Add(ScanReportType.ExposedSecretReport);
            }

            if (reports.SbomReport is not null)
            {
                SbomReport = reports.SbomReport;
                Present.Add(ScanReportType.SbomReport);
            }
        }
    }
}
