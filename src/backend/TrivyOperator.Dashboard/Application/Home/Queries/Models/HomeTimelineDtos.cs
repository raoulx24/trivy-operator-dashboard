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
// - an image counts only if it has at least one pair inside the severities filter (an image without any is not here,
//   so every counted day has rows in the flat form)
public sealed record HomeNewImagesDayDto(
    DateOnly Day,
    int Images,
    int Vulnerabilities,
    HomeSeverityCountsDto VulnerabilitiesBySeverity,
    int KnownExploited);

public sealed record HomeNewImagesDto(
    bool IsAvailable,
    string? UnavailableReason,
    DateOnly From,
    DateTime? HistoryStartedAt,
    IReadOnlyList<HomeNewImagesDayDto> Days);

// the flat form of the new images
// - Items: one row per new image (digest) and (vulnerability id, severity) it has now
// - Groups: one per (vulnerability id, severity), the distinct pairs the days count; Count = the sum of the Count of its
//   items, Images = the new images that have it, FirstSeenAt = the first of them, Namespaces = the union of theirs
//   (where they run now, inside the namespaces filter); Title: of the vulnerability definition
public sealed record HomeNewImagesDenormalizedItemDto(
    string VulnerabilityId,
    int SeverityId,
    // the moment the digest was first seen by the history
    DateTime FirstSeenAt,
    string ImageDigest,
    string Registry,
    string Repository,
    string Tag,
    // the occurrences (package, version, target) of the pair in the image, now
    int Count,
    // of the image (the namespaces filter applies): the workloads, the namespaces count, and the first names (sorted,
    // capped)
    int Workloads,
    int Namespaces,
    IReadOnlyList<string> NamespaceNames);

public sealed record HomeNewImagesGroupDto(
    string VulnerabilityId,
    int SeverityId,
    string? Title,
    int Count,
    int Images,
    DateTime FirstSeenAt,
    bool IsKnownExploited,
    double? EpssPercentile,
    int Namespaces,
    IReadOnlyList<string> NamespaceNames);

public sealed record HomeNewImagesDenormalizedDto(
    bool IsAvailable,
    string? UnavailableReason,
    DateOnly From,
    // first seen first, the most recent last
    IReadOnlyList<HomeNewImagesDenormalizedItemDto> Items,
    IReadOnlyList<HomeNewImagesGroupDto> Groups);

// --- summaries of the changes widgets (new vulnerabilities, new fixes), per UTC day: the counts of the rows that the
// list endpoints return (limited), but of all of them, for the strip "since your last visit"
// - ActiveVulnerabilities: the rows (vulnerability id, package) as the list counts them (Totals.ActiveVulnerabilities);
//   VulnerabilityIds: the distinct ids among them
// - the severities and namespaces filters apply; the range is the days ending today (UTC), not a Since
public sealed record HomeDaySummaryDto(
    DateOnly Day,
    int ActiveVulnerabilities,
    int VulnerabilityIds);

// the active vulnerabilities first seen per day (FirstAt: a row that existed before the history started is never new)
public sealed record HomeNewVulnerabilitiesSummaryDto(
    bool IsAvailable,
    string? UnavailableReason,
    DateOnly From,
    IReadOnlyList<HomeDaySummaryDto> Days);

// the active vulnerabilities whose fix appeared, per day (FixAppearedAt; the ones that came with their fix are not here)
public sealed record HomeNewFixesSummaryDto(
    bool IsAvailable,
    string? UnavailableReason,
    DateOnly From,
    IReadOnlyList<HomeDaySummaryDto> Days);

// the known exploited (KEV / EUVD) vulnerabilities per day they became known exploited (KnownExploitedSince: the earliest
// of the KEV addition and the EUVD exploitation moment, the one the known exploited widget calls new), among the active
// ones in the scope; the severities do not apply (as in the widget), ActiveVulnerabilities are the rows of the ids
public sealed record HomeKnownExploitedSummaryDto(
    bool IsAvailable,
    string? UnavailableReason,
    DateOnly From,
    IReadOnlyList<HomeDaySummaryDto> Days);
