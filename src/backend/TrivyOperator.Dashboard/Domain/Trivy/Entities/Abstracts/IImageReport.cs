using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;

public interface IImageReport<TSelf> : ITrivyReport<TSelf, Digest>, IImageReport
where TSelf : IImageReport<TSelf>
{
    TSelf WithOccurrences(IReadOnlyList<ReportImageOccurrence> occurrences);
}

public interface IImageReport : ITrivyReport
{
    IReadOnlyList<ReportImageOccurrence> Occurrences { get; }
    Digest ImageDigest { get; }
}