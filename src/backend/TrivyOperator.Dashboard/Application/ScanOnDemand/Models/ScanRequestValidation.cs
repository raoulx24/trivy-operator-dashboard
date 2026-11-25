using TrivyOperator.Dashboard.Domain.ScanOnDemand.ValueObjects;

namespace TrivyOperator.Dashboard.Application.ScanOnDemand.Models;

// the outcome of checking a scan request; Image is null when the reference could not be parsed
public sealed record ScanRequestValidation(
    ImageReference? Image,
    IReadOnlySet<ScanReportType> ReportTypes,
    IReadOnlyList<string> Errors)
{
    public bool IsValid => Image is not null && ReportTypes.Count > 0 && Errors.Count == 0;
}
