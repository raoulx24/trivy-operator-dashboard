using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

public readonly record struct ReportMetadata(
    ResourceName Name,
    NamespaceName NamespaceName,
    Timestamp CreationTimestamp,
    Uid Uid,
    IReadOnlyList<OwnerReference> OwnerReferences
)
{
    public ReportMetadata() : this(new ResourceName(), new NamespaceName(), new Timestamp(), new Uid(), [])
    { }
}
