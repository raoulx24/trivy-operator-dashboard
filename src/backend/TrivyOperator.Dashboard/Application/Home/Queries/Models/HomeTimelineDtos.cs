namespace TrivyOperator.Dashboard.Application.Home.Queries.Models;

// the timelines of the home page, per UTC day, from the vulnerability reports history
// both count distinct (vulnerability id, severity) pairs; severities and namespaces filters apply, the window (Since)
// does not (they have their own range of days)

// distinct (vulnerability id, severity) pairs per severity; they sum up to the total next to them
// (only the requested severities have values, the others are 0)
public sealed record HomeSeverityCountsDto(
    int Critical,
    int High,
    int Medium,
    int Low,
    int Unknown);

// --- history drift: what was added to images that were already running (the snapshots' cvesDeltas, Added only)
// - a new digest's first snapshot has no deltas: what a new image brings is in the new images timeline
// - a pair is counted once per day it was added to any image in the range (a pair added to more images on a later
//   day counts again on that day; its severity as it was added; a severity change of a CVE is a new pair), and only if
//   the vulnerability id is still active in a running image
//   so the days do not sum to the distinct pairs; the day a pair was first added is FirstAddedAt of its group
// - complete only within the history retention (KeepDays): older deltas of changing images are pruned
public sealed record HomeHistoryDriftDayDto(
    DateOnly Day,
    int Added,
    HomeSeverityCountsDto AddedBySeverity,
    int AddedKnownExploited);

public sealed record HomeHistoryDriftDto(
    bool IsAvailable,
    string? UnavailableReason,
    DateOnly From,
    IReadOnlyList<HomeHistoryDriftDayDto> Days);

// the flat form of the history drift
// - Items: one row per history entry (image digest + cves hash, of a running image) and (vulnerability id, severity)
//   added in it, as the history has them: AddedAt (the first seen of the entry), the image, the namespaces; the
//   severity is as it was added
//   a pair can appear in several entries of a digest (even on the same day): summing the Count is up to the reader
// - Groups: one per (vulnerability id, severity), the distinct pairs the days count; Count = the sum of the Count of
//   its items, FirstAddedAt = the first of them (the day the pair first appeared in the range), Namespaces = the union
//   of theirs; Title: of the vulnerability definition (null when the definitions are not available)
public sealed record HomeHistoryDriftDenormalizedItemDto(
    string VulnerabilityId,
    int SeverityId,
    DateTime AddedAt,
    string ImageDigest,
    string Registry,
    string Repository,
    string Tag,
    // the occurrences (package, version, target) of the pair in the entry, as the history counts them
    int Count,
    // of the history entry (the namespaces filter applies): the count, and the first names (sorted, capped)
    int Namespaces,
    IReadOnlyList<string> NamespaceNames);

public sealed record HomeHistoryDriftGroupDto(
    string VulnerabilityId,
    int SeverityId,
    string? Title,
    int Count,
    // the first moment the pair was added to any image in the range
    DateTime FirstAddedAt,
    bool IsKnownExploited,
    double? EpssPercentile,
    // where the images of the pair run now: the count, and the first names (sorted, capped)
    int Namespaces,
    IReadOnlyList<string> NamespaceNames);

public sealed record HomeHistoryDriftDenormalizedDto(
    bool IsAvailable,
    string? UnavailableReason,
    DateOnly From,
    // first added first, the most recent last
    IReadOnlyList<HomeHistoryDriftDenormalizedItemDto> Items,
    IReadOnlyList<HomeHistoryDriftGroupDto> Groups);

// --- new images: the running images by the day their digest was first seen by the history, with what they have now
// (current state: the vulnerabilities and severities of today's reports, not of the moment they arrived)
// - per day the pairs are distinct across the images of that day; the same pair on two days counts on both (each
//   deployment brought it)
public sealed record HomeNewImagesDayDto(
    DateOnly Day,
    int Images,
    int Vulnerabilities,
    HomeSeverityCountsDto VulnerabilitiesBySeverity,
    int KnownExploited);

public sealed record HomeNewImageDto(
    string Digest,
    string Registry,
    string Repository,
    string Tag,
    DateTime FirstSeenAt,
    // distinct (vulnerability id, severity) pairs of the image, now
    int Vulnerabilities,
    HomeSeverityCountsDto VulnerabilitiesBySeverity,
    int KnownExploited,
    int Workloads,
    int Namespaces,
    IReadOnlyList<string> NamespaceNames);

public sealed record HomeNewImagesDto(
    bool IsAvailable,
    string? UnavailableReason,
    DateOnly From,
    DateTime? HistoryStartedAt,
    IReadOnlyList<HomeNewImagesDayDto> Days,
    // the newest first
    IReadOnlyList<HomeNewImageDto> Items);
