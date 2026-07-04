using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Abstracts;

namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

public sealed record ReportImageOccurrence(ReportMetadata Metadata, Resource Resource, ImageMeta ImageMeta) : IReportOccurrence;
