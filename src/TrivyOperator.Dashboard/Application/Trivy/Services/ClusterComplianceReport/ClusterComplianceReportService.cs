using TrivyOperator.Dashboard.Application.Trivy.Models;
using TrivyOperator.Dashboard.Application.Trivy.Services.ClusterComplianceReport.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.ClusterComplianceReport;
using TrivyOperator.Dashboard.Infrastructure.Caching.Abstractions;

namespace TrivyOperator.Dashboard.Application.Trivy.Services.ClusterComplianceReport;

public class ClusterComplianceReportService(IConcurrentDictionaryCache<ClusterComplianceReportCr> cache)
    : IClusterComplianceReportService
{
    public Task<IEnumerable<ClusterComplianceReportDto>> GetClusterComplianceReportDtos()
    {
        IEnumerable<ClusterComplianceReportCr> cachedValues = [.. cache.SelectMany(kvp => kvp.Value.Values),];
        IEnumerable<ClusterComplianceReportDto> values = cachedValues
            .Select(x => x.ToClusterComplianceReportDto());

        return Task.FromResult(values);
    }

    public Task<IEnumerable<ClusterComplianceReportDenormalizedDto>> GetClusterComplianceReportDenormalizedDtos()
    {
        IEnumerable<ClusterComplianceReportCr> cachedValues = [.. cache.SelectMany(kvp => kvp.Value.Values),];
        IEnumerable<ClusterComplianceReportDenormalizedDto> values = cachedValues
            .SelectMany(x => x.ToClusterComplianceReportDenormalizedDtos());

        return Task.FromResult(values);
    }
}
