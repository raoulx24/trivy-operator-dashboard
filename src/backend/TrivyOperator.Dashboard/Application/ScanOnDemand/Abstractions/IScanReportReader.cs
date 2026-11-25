using TrivyOperator.Dashboard.Application.ScanOnDemand.Models;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.ScanOnDemand.ValueObjects;

namespace TrivyOperator.Dashboard.Application.ScanOnDemand.Abstractions;

// reads the trivy reports owned by a scan pod (by the trivy-operator.resource.name label), only the given types
public interface IScanReportReader
{
    Task<ScanReports> ReadAsync(
        ResourceName podName,
        IReadOnlySet<ScanReportType> types,
        CancellationToken ctx = default);
}
