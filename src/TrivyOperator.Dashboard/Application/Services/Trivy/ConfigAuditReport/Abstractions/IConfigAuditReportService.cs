﻿using TrivyOperator.Dashboard.Application.Models;

namespace TrivyOperator.Dashboard.Application.Services.Trivy.ConfigAuditReport.Abstractions;

public interface IConfigAuditReportService
{
    Task<IList<ConfigAuditReportDenormalizedDto>> GetConfigAuditReportDenormalizedDtos(string? namespaceName = null);

    Task<IEnumerable<ConfigAuditReportDto>> GetConfigAuditReportDtos(
        string? namespaceName = null,
        IEnumerable<int>? excludedSeverities = null);

    Task<IEnumerable<string>> GetActiveNamespaces();
    public Task<IEnumerable<ConfigAuditReportSummaryDto>> GetConfigAuditReportSummaryDtos();
}
