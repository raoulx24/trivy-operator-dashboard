using TrivyOperator.Dashboard.Domain.History.NamespaceHistory.ValueObjects;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.ValueObjects;

namespace TrivyOperator.Dashboard.Domain.History.NamespaceHistory.Abstractions;

public interface INamespaceHistoryStore
{
    Task<IReadOnlyList<NamespaceName>> GetNamespacesAsync(CancellationToken ct = default);
    Task AddOrUpdateNamespaceAsync(NamespaceName namespaceName, CancellationToken ct = default);
    Task DeleteNamespacesAsync(IEnumerable<NamespaceName> namespaceNames, CancellationToken ct = default);
}
