using TrivyOperator.Dashboard.Domain.EndOfLife.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

namespace TrivyOperator.Dashboard.Domain.EndOfLife.Entities;

// one release cycle of an endoflife.date product
// Eoas (end of active support) and Eoes (end of extended support) are null when the product has no such phase
// IsEoes is also null when the release cycle is not eligible for extended support
public sealed record EolRelease(
    EolCycle Cycle,
    string? Codename,
    string Label,
    DateOnly? ReleaseDate,
    bool IsLts,
    DateOnly? LtsFrom,
    bool IsEol,
    DateOnly? EolFrom,
    bool IsMaintained,
    bool? IsEoas,
    DateOnly? EoasFrom,
    bool? IsEoes,
    DateOnly? EoesFrom,
    EolLatestVersion? Latest);

public sealed record EolLatestVersion(
    string Name,
    DateOnly? Date,
    ResourceUrl Link);
