using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;

public interface IImageReport<out TSelf>
    : ITrivyReport<Digest>
    where TSelf : IImageReport<TSelf>
{
    Digest ImageDigest { get; }
    IReadOnlyList<ReportImageOccurrence> Occurrences { get; }
    TSelf WithOccurrences(IReadOnlyList<ReportImageOccurrence> occurrences);
}
