using TrivyOperator.Dashboard.Application.TrivyReportDependencies.Models;

namespace TrivyOperator.Dashboard.Application.TrivyReportDependencies.Services.Abstractions;
public interface ITrivyReportDependenciesService
{
    Task<TrivyReportDependencyDto?> GetTryvyReportDependencies(string imageDigest, string namespaceName);
}