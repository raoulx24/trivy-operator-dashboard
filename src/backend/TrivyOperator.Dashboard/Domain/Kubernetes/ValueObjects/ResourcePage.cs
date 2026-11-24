namespace TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

// one page of a paged list: valid items, skipped (malformed) items, continue token and list resourceVersion
public sealed record ResourcePage<TKubernetesObject>(
    IReadOnlyList<TKubernetesObject> Items,
    IReadOnlyList<MalformedObject> Malformed,
    string? ContinueToken,
    string? ResourceVersion
);
