using System.Collections.Frozen;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.ScanOnDemand.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.ScanOnDemand.Entities;

// one scan request and its lifecycle; small on purpose, the reports live in OnDemandScanResult
public sealed record OnDemandScan(
    ScanId Id,
    ImageReference Image,
    IReadOnlySet<ScanReportType> RequestedReportTypes,
    string? RequestedBy,
    Timestamp CreatedAt)
{
    public ScanState State { get; init; } = ScanState.Pending;
    public Timestamp UpdatedAt { get; init; } = CreatedAt;
    // when the pod was created; the timeout counts from here
    public Timestamp? StartedAt { get; init; }
    public Timestamp? FinishedAt { get; init; }

    // set when the pod is created
    public ResourceName PodName { get; init; } = new();

    // known after the first report
    public Digest ImageDigest { get; init; } = new(null);

    // the requested types that actually arrived
    public IReadOnlySet<ScanReportType> ProducedReportTypes { get; init; } = FrozenSet<ScanReportType>.Empty;

    public string? FailureMessage { get; init; }

    public bool IsFinal => State is ScanState.Completed or ScanState.Failed or ScanState.TimedOut;

    public OnDemandScan MarkRunning(ResourceName podName, Timestamp now)
    {
        EnsureState(ScanState.Pending);

        return this with { State = ScanState.Running, PodName = podName, UpdatedAt = now, StartedAt = now, };
    }

    public OnDemandScan MarkCompleted(Digest digest, IReadOnlySet<ScanReportType> produced, Timestamp now)
    {
        EnsureState(ScanState.Running);

        return this with
        {
            State = ScanState.Completed,
            ImageDigest = digest,
            ProducedReportTypes = produced,
            UpdatedAt = now,
            FinishedAt = now,
        };
    }

    // allowed from Pending too: the pod creation itself can be rejected
    public OnDemandScan MarkFailed(string message, Timestamp now)
    {
        EnsureState(ScanState.Pending, ScanState.Running);

        return this with { State = ScanState.Failed, FailureMessage = message, UpdatedAt = now, FinishedAt = now, };
    }

    public OnDemandScan MarkTimedOut(Timestamp now)
    {
        EnsureState(ScanState.Running);

        return this with { State = ScanState.TimedOut, UpdatedAt = now, FinishedAt = now, };
    }

    private void EnsureState(params ScanState[] allowed)
    {
        if (!allowed.Contains(State))
        {
            throw new InvalidOperationException(
                $"Scan {Id} is {State}; expected {string.Join(" or ", allowed)}.");
        }
    }
}
