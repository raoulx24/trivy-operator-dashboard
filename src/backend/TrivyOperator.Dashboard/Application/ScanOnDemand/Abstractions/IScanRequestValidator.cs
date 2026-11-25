using TrivyOperator.Dashboard.Application.ScanOnDemand.Models;
using TrivyOperator.Dashboard.Domain.ScanOnDemand.ValueObjects;

namespace TrivyOperator.Dashboard.Application.ScanOnDemand.Abstractions;

// checks a request: parses the image, applies the registry allow and deny lists, and checks the requested
// report types against the allowed ones (at least one, none outside the ceiling)
public interface IScanRequestValidator
{
    // the ceiling from the settings, unknown names left out
    IReadOnlySet<ScanReportType> AllowedReportTypes { get; }


    ScanRequestValidation Validate(string? imageReference, IReadOnlyCollection<ScanReportType> requested);
}
