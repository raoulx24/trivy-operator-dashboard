namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Sboms;

public sealed record Supplier(
    string? Name,
    string? Email,
    string? Phone);
