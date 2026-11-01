namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

public readonly record struct ImageLayer(Digest DiffId, Digest Digest);
