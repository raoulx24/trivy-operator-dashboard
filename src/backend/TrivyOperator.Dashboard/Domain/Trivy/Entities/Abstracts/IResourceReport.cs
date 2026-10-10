using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;

public interface IResourceReport<TSelf> : ITrivyReport<TSelf, Uid>
where TSelf : IResourceReport<TSelf>
{
    ReportMetadata Metadata { get; }
}
