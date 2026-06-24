using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.ValueObjects;

namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

public sealed record ImageUsage(
    Digest Digest,
    IReadOnlyList<ImageMeta> Metas
);
