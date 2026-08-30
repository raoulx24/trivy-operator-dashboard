
using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Services.TrivyReportDependencies.Abstractions;

public interface ITrivyReportDependenciesService
{
    Task<TrivyDependencyTreeDto?> GetTrivyDependencyTree(
        string imageDigest,
        string? namespaceName = null,
        CancellationToken ctx = default
    );

    Task<bool> TrivyDependenciesExist(
        string imageDigest,
        string? namespaceName = null,
        CancellationToken ct = default
    );
}
