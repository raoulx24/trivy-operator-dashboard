namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Sboms;

public sealed record License(
    string? Id,
    string? Name,
    Uri? Url);
