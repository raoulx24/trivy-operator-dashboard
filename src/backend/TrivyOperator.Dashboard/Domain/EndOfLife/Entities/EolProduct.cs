using TrivyOperator.Dashboard.Domain.EndOfLife.Services;
using TrivyOperator.Dashboard.Domain.EndOfLife.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

namespace TrivyOperator.Dashboard.Domain.EndOfLife.Entities;

public sealed record EolProduct(
    ProductSlug Slug,
    string Label,
    ResourceUrl Link,
    ResourceUrl ReleasePolicyLink,
    EolPhaseLabels Labels,
    IReadOnlyList<EolRelease> Releases,
    Timestamp? LastModified)
{
    public EolRelease? FindRelease(string version) => EolCycleMatcher.Match(Releases, version);
}

// product specific display texts for the lifecycle phases (e.g. "Security Support" for Eoes)
public sealed record EolPhaseLabels(
    string Eol,
    string? Eoas,
    string? Eoes);
