using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;

public interface IImageReport : ITrivyReport<Digest>, IHasSeverityCounters
{
    Digest ImageDigest { get; }
    IReadOnlyList<ReportImageOccurrence> Occurrences { get; }
}

public interface IImageReport<out TSelf>
    : IImageReport
    where TSelf : IImageReport<TSelf>
{
    TSelf WithOccurrences(IReadOnlyList<ReportImageOccurrence> occurrences);
}
