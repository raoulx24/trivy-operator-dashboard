namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

public sealed record ImageMeta(
    string Registry,
    string Repo,
    string Name,
    string Tag
);