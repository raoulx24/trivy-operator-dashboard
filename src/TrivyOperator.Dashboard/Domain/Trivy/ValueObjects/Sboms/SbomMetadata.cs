namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Sboms;

public sealed record SbomMetadata(
    string BomFormat,
    string SpecVersion,
    long Version,
    DateTime? GeneratedAt);
