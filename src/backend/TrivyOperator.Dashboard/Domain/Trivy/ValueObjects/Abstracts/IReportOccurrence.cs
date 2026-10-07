using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Abstracts;

public interface IReportOccurrence
{
    ReportMetadata Metadata { get; }
}
