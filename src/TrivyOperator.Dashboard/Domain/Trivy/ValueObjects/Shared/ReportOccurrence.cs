using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Abstracts;

namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

public abstract record ReportOccurrence(ReportMetadata Metadata) : IReportOccurrence;
