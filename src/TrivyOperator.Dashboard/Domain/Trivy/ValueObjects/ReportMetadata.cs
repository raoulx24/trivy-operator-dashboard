using TrivyOperator.Dashboard.Domain.History.NamespaceHistory.ValueObjects;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.ValueObjects;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects;

public readonly record struct ReportMetadata(
    ResourceName Name,
    Kind Kind,
    NamespaceName NamespaceName,
    Timestamp CreationTimestamp,
    Guid Guid
);
