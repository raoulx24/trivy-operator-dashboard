using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;

public interface IResourceReport : ITrivyReport<Uid>
{
    ReportMetadata Metadata { get; }
}
