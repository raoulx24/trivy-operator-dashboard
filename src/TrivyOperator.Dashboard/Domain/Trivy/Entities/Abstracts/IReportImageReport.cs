using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;

public interface IReportImageReport : ITrivyReport
{
    IReadOnlyList<ReportImageOccurrence> Occurrences { get; }
}
