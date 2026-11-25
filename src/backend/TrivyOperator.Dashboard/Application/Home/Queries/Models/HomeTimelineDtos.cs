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

// --- new images: the running images by the day their digest was first seen by the history, with what they have now
// (current state: the vulnerabilities and severities of today's reports, not of the moment they arrived)
// - per day the pairs are distinct across the images of that day; the same pair on two days counts on both (each
//   deployment brought it)
// - an image counts only if it has at least one pair inside the severities filter (an image without any is not here,
//   so every counted day has rows in the normalized form)
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

// --- the normalized form of the history drift and the new images (what their boxes and graph are made of): nothing is
// repeated, as in the new vulnerabilities list. The namespaces, the pairs and the images are lists, and the rest points
// into them by index. The "Show all" tables flatten it on the client (a row per image and pair, the namespaces as an
// array column).
// - NamespaceNames: every namespace of the response, sorted, not capped
// - Pairs: every (vulnerability id, severity) of the response, the worst severity first, then by id

// a (vulnerability id, severity) of a normalized response; Title: of the vulnerability definition (null when the
// definitions are not available)
public sealed record HomePivotPairDto(
    string VulnerabilityId,
    int SeverityId,
    string? Title,
    bool IsKnownExploited,
    double? EpssPercentile);

// a pair of an image (or an entry): an index in the Pairs of the response, and the occurrences (package, version, target)
// of the pair (as the history counts them for the history drift)
public sealed record HomePivotPairCountDto(int PairIndex, int Count);

public sealed record HomeHistoryDriftNormalizedImageDto(string Digest, string Registry, string Repository, string Tag);

// a history entry: the pairs added to an image at a moment (the history entries of a digest at the same moment are one);
// NamespaceIndexes: the namespaces of the entry (the namespaces filter applies), indexes in NamespaceNames
public sealed record HomeHistoryDriftNormalizedEntryDto(
    int ImageIndex,
    DateTime AddedAt,
    IReadOnlyList<int> NamespaceIndexes,
    IReadOnlyList<HomePivotPairCountDto> Pairs);

// Entries: the oldest first; Images: the images of the entries, in the order of their first entry
public sealed record HomeHistoryDriftNormalizedDto(
    bool IsAvailable,
    string? UnavailableReason,
    DateOnly From,
    IReadOnlyList<string> NamespaceNames,
    IReadOnlyList<HomePivotPairDto> Pairs,
    IReadOnlyList<HomeHistoryDriftNormalizedImageDto> Images,
    IReadOnlyList<HomeHistoryDriftNormalizedEntryDto> Entries);

// a new image with the pairs it has now; NamespaceIndexes: where it runs (the namespaces filter applies)
public sealed record HomeNewImagesNormalizedImageDto(
    string Digest,
    string Registry,
    string Repository,
    string Tag,
    // the moment the digest was first seen by the history
    DateTime FirstSeenAt,
    int Workloads,
    IReadOnlyList<int> NamespaceIndexes,
    IReadOnlyList<HomePivotPairCountDto> Pairs);

// Images: the oldest first
public sealed record HomeNewImagesNormalizedDto(
    bool IsAvailable,
    string? UnavailableReason,
    DateOnly From,
    IReadOnlyList<string> NamespaceNames,
    IReadOnlyList<HomePivotPairDto> Pairs,
    IReadOnlyList<HomeNewImagesNormalizedImageDto> Images);

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
