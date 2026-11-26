using System.Collections.Concurrent;
using TrivyOperator.Dashboard.Application.ScanOnDemand.Abstractions;
using TrivyOperator.Dashboard.Domain.ScanOnDemand.ValueObjects;

namespace TrivyOperator.Dashboard.Application.ScanOnDemand.Services;

public sealed class OnDemandScanCancellations : IOnDemandScanCancellations
{
    private readonly ConcurrentDictionary<ScanId, byte> requests = new();

    public void Request(ScanId id) => requests.TryAdd(id, 0);

    public bool TryTake(ScanId id) => requests.TryRemove(id, out _);
}
