
using TrivyOperator.Dashboard.Application.TrivyDependencies.Queries.Models;

namespace TrivyOperator.Dashboard.Application.TrivyDependencies.Queries.Services.Abstractions;

public interface ITrivyReportDependenciesService
{
    Task<TrivyDependencyTreeDto?> GetTrivyDependencyTree(
        string imageDigest,
        string? namespaceName = null,
        CancellationToken ctx = default
    );

    Task<bool> TrivyDependenciesExist(
        string imageDigest,
        CancellationToken ct = default
    );
}
