using TrivyOperator.Dashboard.Domain.History.NamespaceHistory.ValueObjects;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.ValueObjects;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects;

public readonly record struct ReportMetadata(
    ResourceName Name,
    Kind Kind,
    NamespaceName NamespaceName,
    Timestamp CreationTimestamp,
    Guid Guid
)
{
    public ReportMetadata ValidateKind(Kind expectedKind)
    {
        if (Kind != expectedKind)
            throw new ArgumentException(
                $"Invalid report kind. Expected '{expectedKind}', got '{Kind}'."
            );

        return this;
    }

    public ReportMetadata ValidateNamespace(bool mustBeClusterScoped)
    {
        if (mustBeClusterScoped && !NamespaceName.IsClusterScoped)
            throw new ArgumentException(
                "Cluster-scoped reports must not have a namespace."
            );

        if (!mustBeClusterScoped && NamespaceName.IsClusterScoped)
            throw new ArgumentException(
                "Namespaced reports must have a namespace."
            );

        return this;
    }
}
