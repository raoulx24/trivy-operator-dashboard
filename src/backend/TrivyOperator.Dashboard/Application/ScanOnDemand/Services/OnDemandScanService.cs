using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.ScanOnDemand.Abstractions;
using TrivyOperator.Dashboard.Application.ScanOnDemand.Models;
using TrivyOperator.Dashboard.Application.ScanOnDemand.Options;
using TrivyOperator.Dashboard.Domain.ScanOnDemand.Entities;
using TrivyOperator.Dashboard.Domain.ScanOnDemand.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.ScanOnDemand.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Application.ScanOnDemand.Services;

// a submit only records the scan (Pending) and wakes the runner; the runner does the work
public sealed class OnDemandScanService(
    IScanRequestValidator validator,
    IOnDemandScanStore store,
    IOnDemandScanWakeUp wakeUp,
    IOnDemandScanNotifier notifier,
    IOnDemandScanCancellations cancellations,
    IOptions<ScanOnDemandOptions> options,
    ILogger<OnDemandScanService> logger
) : IOnDemandScanService
{
    public async Task<ScanSubmitResult> SubmitAsync(
        string? imageReference,
        IReadOnlyCollection<ScanReportType> types,
        string? requestedBy,
        CancellationToken ctx = default)
    {
        ScanRequestValidation validation = validator.Validate(imageReference, types);

        if (!validation.IsValid)
        {
            return new ScanSubmitResult(null, validation.Errors);
        }

        ImageReference image = validation.Image!;
        OnDemandScan[] active = [.. (await store.GetScansAsync(ctx)).Where(static x => !x.IsFinal),];

        // the same request while one is pending or running: that one (not atomic; two simultaneous submits may both pass)
        OnDemandScan? existing = active.FirstOrDefault(x =>
            x.Image == image && x.RequestedReportTypes.SetEquals(validation.ReportTypes));

        if (existing is not null)
        {
            return new ScanSubmitResult(existing.Id, [], true);
        }

        if (active.Length >= options.Value.MaxQueuedScans)
        {
            return new ScanSubmitResult(
                null,
                [$"There are already {active.Length} scans waiting or running; try again later.",]);
        }

        OnDemandScan scan = new(
            ScanId.New(),
            image,
            validation.ReportTypes,
            string.IsNullOrWhiteSpace(requestedBy) ? null : requestedBy.Trim(),
            Timestamp.Now());

        await store.SaveScanAsync(scan, ctx);
        wakeUp.Signal();
        await notifier.ScansChangedAsync(ctx);

        logger.LogInformation(
            "Scan {scanId} of {image} ({reportTypes}) submitted",
            scan.Id,
            image.FullName,
            string.Join(", ", scan.RequestedReportTypes));

        return new ScanSubmitResult(scan.Id, []);
    }

    public Task<OnDemandScan?> GetScanAsync(ScanId id, CancellationToken ctx = default)
        => store.GetScanAsync(id, ctx);

    public async Task<ScanCancelResult> CancelScanAsync(ScanId id, CancellationToken ctx = default)
    {
        OnDemandScan? scan = await store.GetScanAsync(id, ctx);

        if (scan is null)
        {
            return ScanCancelResult.NotFound;
        }

        if (scan.IsFinal)
        {
            return ScanCancelResult.AlreadyFinished;
        }

        cancellations.Request(scan.Id);
        wakeUp.Signal();
        logger.LogInformation("Cancel requested for scan {scanId} of {image}", scan.Id, scan.Image.FullName);

        return ScanCancelResult.Accepted;
    }

    public async Task<ScanDeleteResult> DeleteScanAsync(ScanId id, CancellationToken ctx = default)
    {
        OnDemandScan? scan = await store.GetScanAsync(id, ctx);

        if (scan is null)
        {
            return ScanDeleteResult.NotFound;
        }

        if (!scan.IsFinal)
        {
            return ScanDeleteResult.NotFinished;
        }

        await store.DeleteScanAsync(scan, ctx);
        logger.LogInformation("Scan {scanId} of {image} deleted", scan.Id, scan.Image.FullName);
        await notifier.ScansChangedAsync(ctx);

        return ScanDeleteResult.Deleted;
    }

    public Task<IReadOnlyList<OnDemandScan>> GetScansAsync(CancellationToken ctx = default)
        => store.GetScansAsync(ctx);

    public Task<OnDemandScanResult?> GetResultAsync(Digest digest, CancellationToken ctx = default)
        => store.GetResultAsync(digest, ctx);

    // the scans pointing to the digest go too, so the list changes
    public async Task DeleteResultAsync(Digest digest, CancellationToken ctx = default)
    {
        await store.DeleteResultAsync(digest, ctx);
        await notifier.ScansChangedAsync(ctx);
    }
}
