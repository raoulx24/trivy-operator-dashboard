using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

public sealed record ResourceOccurrence(ReportMetadata Metadata, Resource Resource);
