using TrivyOperator.Dashboard.Application.TrivyReportDependencies.Models;

namespace TrivyOperator.Dashboard.Application.TrivyReportDependencies.Services.Abstractions;

public interface ITrivyReportDependenciesService
{
    Task<TrivyReportDependencyDto?> GetTrivyReportDependencies(string imageDigest, string namespaceName);
}
