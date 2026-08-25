namespace TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

public readonly record struct OwnerReference(Uid Uid, Kind Kind, ResourceName Name)
{
    public OwnerReference() : this(new Uid(), new Kind(), new ResourceName())
    { }
}
