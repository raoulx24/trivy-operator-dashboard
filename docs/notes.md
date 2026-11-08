# Notes

## Vulnerability Reports: a missing vulnerability detail shows as severity Unknown, and since 2.0.0 it also changes the severity counters

**Where:** the live Vulnerability Reports views (and Cluster Vulnerability Reports). Vulnerability Reports History is **not** affected.

### Background: how a vulnerability report is stored and read

To save memory, the backend does not keep each Vulnerability Report (VR) in full. It splits it:

- a **slim report** (`VulnerabilityReportSlim` / `ClusterVulnerabilityReportSlim`), with the image and resource data, the severity counters, and, for each finding, only the CVE id and the package (name, version, target, purl);
- two shared in-memory **detail stores**, filled by `VulnerabilityDetailsCollector` before the slim report is stored:
  - `VulnerabilityDefinition`, keyed by CVE id: title, primary link, references, published and modified dates;
  - `PackageVulnerability`, keyed by CVE id + `PackageKey` (the package without its version): **severity**, score, fixed version.

When the API needs the full report, `HydratingVulnerabilityReportProvider` rebuilds the findings by looking up each one in the two stores ("hydration").

A housekeeping job removes the details that no stored report references any more.

### What happens when a detail is missing

If a lookup finds nothing, the finding is not dropped. `VulnerabilitySlimExtensions.Hydrate` uses a placeholder:

- missing definition: title = the CVE id, no link, no references, no dates;
- missing package vulnerability: **severity Unknown**, no score, no fixed version.

Since commit `d936658b`, `HydratingVulnerabilityReportProvider` also **recomputes the report's severity counters** from the hydrated findings (`WithSeverityCounters`). Before, the counters were the ones Trivy wrote in the report. The goal is that the counters follow the severities currently in the detail store, which can change after the report was stored, for example when a newer report reclassifies a CVE.

The side effect: a finding whose detail is missing now also moves to the Unknown bucket of the counters, for example High −1 and Unknown +1. Before that commit, only the detail row showed Unknown.

### Related: summaries keep the stored counters

`GetResourceSummaries` / `GetResourceSummary` do not hydrate, so they return the counters stored with the slim report (Trivy's numbers). `GetResources` / `GetResource` return the recomputed ones. When the two differ (a reclassified severity, or a missing detail as above):

- the severity filter in `TrivyQuerySupport.GetResources` (`excludedSeverities`) selects reports by their summary counters, then returns the full reports with the recomputed counters. A report can be filtered in or out by numbers that differ from the ones it then shows;
- the SBOM list takes the VR severity counters from the summaries, and the SBOM detail from the full report, so the two can disagree.

### How likely is it

In normal operation a detail should not be missing for a stored report:

- the details are collected before the slim report is stored;
- housekeeping keeps every detail that a stored report references.

It can still happen in a race between housekeeping and a new report, or because of a bug in housekeeping. If you see an unexpected Unknown finding, or a counter that disagrees between a list and a detail view, this is the first place to look. Nothing is planned for it at the moment.

### Why VR History is not affected

Snapshots in Vulnerability Reports History are stored in Redis with a full copy of each finding (`VulnerabilityPersistenceModel`: severity, score, title, links, dates, fixed version...). They are built from the complete report that comes with the Kubernetes event, not from the hydrated one, and they never read the detail stores. History only uses the live VRs to decide whether an image digest is still in use (Stale / Fresh / Active), and that needs only the report ids and occurrences.

## SBOM Reports: license expressions are not mapped

CycloneDX writes a component license either as `{ "license": { "id" | "name" | "url" } }` or as `{ "expression": "MIT OR Apache-2.0" }`. `LicenseContainerCr` maps only `license`, so `SbomMappingExtensions.ToLicenses` skips the expression entries: a component licensed by an expression shows no license, in the SBOM views and in the purl index. On one cluster (2026-11-08) there were 1,562 expression entries. Known, and ignored for now.

## Purl index: open points for the query side

The purl index (`PurlDetail` and `ImagePurls` detail stores, family `Sbom`, fed by `PurlDetailsCollector`) has no reader yet. To decide when the query side is built:

- **Deleted images:** the collector does nothing on delete, so an image stays in `ImagePurls` until the housekeeping removes it (`DetailsOptions`: grace 1 h, plus up to one interval, 10 min by default). Until then, "where is this purl" would still list it. Option: filter at query time by the digests still in the SBOM report cache.
- **Only in the default context:** the index is registered only there (it needs the event pipeline), not in multi-context or file repository mode. A reader must treat it as optional, like `InMemoryCveSubjects`.
- **Memory not measured:** compare the backend memory with and without the index on a real cluster.
