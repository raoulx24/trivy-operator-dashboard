using TrivyOperator.Dashboard.Application.Trivy.Models;

namespace TrivyOperator.Dashboard.Application.Trivy.Services.TrivyReportDependencies.Abstractions;

public interface ITrivyReportDependenciesService
{
    Task<TrivyReportDependencyDto?> GetTrivyReportDependencies(string imageDigest, string namespaceName);
}
