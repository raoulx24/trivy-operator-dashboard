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

        logger.LogInformation(
            "Scan {scanId} of {image} ({reportTypes}) submitted",
            scan.Id,
            image.FullName,
            string.Join(", ", scan.RequestedReportTypes));

        return new ScanSubmitResult(scan.Id, []);
    }

    public Task<OnDemandScan?> GetScanAsync(ScanId id, CancellationToken ctx = default)
        => store.GetScanAsync(id, ctx);

    public Task<IReadOnlyList<OnDemandScan>> GetScansAsync(CancellationToken ctx = default)
        => store.GetScansAsync(ctx);

    public Task<OnDemandScanResult?> GetResultAsync(Digest digest, CancellationToken ctx = default)
        => store.GetResultAsync(digest, ctx);

    public Task DeleteResultAsync(Digest digest, CancellationToken ctx = default)
        => store.DeleteResultAsync(digest, ctx);
}
