using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared.Identities;

public readonly record struct NamespacedUid(
    NamespaceName Namespace,
    Uid Uid);
