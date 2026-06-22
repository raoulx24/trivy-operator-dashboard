namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects;

public sealed record ImageMeta(
    string Registry,
    string Repo,
    string Name,
    string Tag
);