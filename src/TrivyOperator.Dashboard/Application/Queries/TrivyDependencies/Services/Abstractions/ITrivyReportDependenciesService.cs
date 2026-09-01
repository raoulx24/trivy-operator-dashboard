
using TrivyOperator.Dashboard.Application.Queries.TrivyDependencies.Models;

namespace TrivyOperator.Dashboard.Application.Queries.TrivyDependencies.Services.Abstractions;

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
