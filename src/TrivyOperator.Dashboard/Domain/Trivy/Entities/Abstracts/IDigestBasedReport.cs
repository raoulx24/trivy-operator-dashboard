using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared.Identities;

namespace TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;

public interface IDigestBasedReport : ITrivyReport
{
    IReadOnlyList<ReportImageOccurrence> Occurrences { get; }
    NamespacedDigest Id { get; }
}
