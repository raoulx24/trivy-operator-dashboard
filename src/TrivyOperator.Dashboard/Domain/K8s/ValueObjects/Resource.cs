using TrivyOperator.Dashboard.Domain.History.NamespaceHistory.ValueObjects;

namespace TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

public readonly record struct Resource(
    ResourceName Name,
    Kind Kind,
    NamespaceName NamespaceName,
    ContainerName? Container
);
