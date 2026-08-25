namespace TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

public readonly record struct Resource(
    ResourceName Name,
    Kind Kind,
    NamespaceName NamespaceName,
    ContainerName? Container,
    IReadOnlyList<OwnerReference>? OwnerReferences
)
{
    public Resource() : this(new ResourceName(), new Kind(), new NamespaceName(), null, null)
    { }
}
