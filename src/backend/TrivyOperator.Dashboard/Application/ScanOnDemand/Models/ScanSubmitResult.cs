using TrivyOperator.Dashboard.Domain.ScanOnDemand.ValueObjects;

namespace TrivyOperator.Dashboard.Application.ScanOnDemand.Models;

// Id is set when the scan was accepted; otherwise Errors says why not
// IsExisting: the same image with the same report types was already pending or running; Id is that scan
public sealed record ScanSubmitResult(ScanId? Id, IReadOnlyList<string> Errors, bool IsExisting = false)
{
    public bool IsAccepted => Id is not null && Errors.Count == 0;
}
