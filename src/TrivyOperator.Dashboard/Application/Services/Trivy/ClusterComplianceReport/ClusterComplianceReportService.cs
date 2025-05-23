﻿using TrivyOperator.Dashboard.Application.Models;
using TrivyOperator.Dashboard.Application.Services.Trivy.ClusterComplianceReport.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.ClusterComplianceReport;
using TrivyOperator.Dashboard.Infrastructure.Abstractions;

namespace TrivyOperator.Dashboard.Application.Services.Trivy.ClusterComplianceReport;

public class ClusterComplianceReportService(IListConcurrentCache<ClusterComplianceReportCr> cache)
    : IClusterComplianceReportService
{
    public Task<IEnumerable<ClusterComplianceReportDto>> GetClusterComplianceReportDtos()
    {
        IEnumerable<ClusterComplianceReportDto> value = cache.SelectMany(kvp => kvp.Value)
            .Select(x => x.ToClusterComplianceReportDto());

        return Task.FromResult(value);
    }

    public Task<IEnumerable<ClusterComplianceReportDenormalizedDto>> GetClusterComplianceReportDenormalizedDtos()
    {
        IEnumerable<ClusterComplianceReportDenormalizedDto> value = cache.SelectMany(kvp => kvp.Value)
            .SelectMany(x => x.ToClusterComplianceReportDenormalizedDto());

        return Task.FromResult(value);
    }
}
