namespace TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

// an object received from Kubernetes that could not be deserialized; it is skipped, but reported
// the identity fields are read best effort from the raw metadata
public sealed record MalformedObject(
    string? Uid,
    string? Name,
    string? Namespace,
    string? ResourceVersion,
    Exception Exception
);
