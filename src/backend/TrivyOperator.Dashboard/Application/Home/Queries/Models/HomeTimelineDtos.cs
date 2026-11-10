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
// - a pair is counted once, on the day it was first added in the range (its severity as it was added; a severity
//   change of a CVE is a new pair), and only if the vulnerability id is still active in a running image
// - complete only within the history retention (KeepDays): older deltas of changing images are pruned
public sealed record HomeHistoryDriftDayDto(
    DateOnly Day,
    int Added,
    HomeSeverityCountsDto AddedBySeverity,
    int AddedKnownExploited);

public sealed record HomeHistoryDriftItemDto(
    string VulnerabilityId,
    // as it was added
    int SeverityId,
    DateTime AddedAt,
    bool IsKnownExploited,
    double? EpssPercentile,
    // the running images it was added to, in the range
    int Images,
    IReadOnlyList<string> ImageNames);

public sealed record HomeHistoryDriftDto(
    bool IsAvailable,
    string? UnavailableReason,
    DateOnly From,
    IReadOnlyList<HomeHistoryDriftDayDto> Days,
    // first added first, the most recent last
    IReadOnlyList<HomeHistoryDriftItemDto> Items);

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
