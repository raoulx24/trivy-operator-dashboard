using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

namespace TrivyOperator.Dashboard.Application.Trivy.SecurityAssessmentDetails.Abstractions;

public interface IDetailReferenceProvider<TDetailId>
{
    // the distinct detail ids referenced by the stored reports, for every context
    Task<IReadOnlyDictionary<ContextName, IReadOnlySet<TDetailId>>> GetReferencedIds(
        CancellationToken ctx = default);
}
