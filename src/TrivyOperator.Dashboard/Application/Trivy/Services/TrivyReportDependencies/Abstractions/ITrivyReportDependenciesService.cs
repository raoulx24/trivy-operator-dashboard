using TrivyOperator.Dashboard.Application.Trivy.Models;

namespace TrivyOperator.Dashboard.Application.Trivy.Services.TrivyReportDependencies.Abstractions;

public interface ITrivyReportDependenciesService
{
    Task<TrivyDependencyTreeDto?> GetTrivyDependencyTreeAsync(
        string imageDigest,
        string namespaceName,
        CancellationToken ct = default
    );

    Task<bool> TrivyDependenciesExistAsync(
        string imageDigest,
        string namespaceName,
        CancellationToken ct = default
    );
}
