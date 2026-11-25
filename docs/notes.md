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

## NVD dialog: the CVSS radar draws every value on the same even scale

The radar in the expanded CVSS rows (`CvssExplanationService.radar`, drawn by `app-radar-chart`) maps each base metric to a radius by the rank of its value: a metric with n values takes the upper n steps of n (the least severe value is one step above the centre, so nothing is drawn at 0 and a vector with several low values does not collapse into spikes). So 2 values are 0.5 and 1, 3 values are 0.33, 0.67 and 1, 4 values are 0.25, 0.5, 0.75 and 1.

The rule does not know what a value means. For example Scope Unchanged (the "nothing special" value) gets 0.5, the same as Attack Complexity High, while OpenCVE draws it around 0.3 (compared by eye on CVE-2026-11850, not measured). Idea, not done: give the 2-value metrics a custom value (for example Scope Unchanged 0.33, Changed 1), or take the numeric weights of the CVSS v3 specification (v4 has none, so it would still need a rule of its own).

Also not done: the v3 axis labels use the short names ("Confidentiality"); NVD and OpenCVE call them "Confidentiality Impact", "Integrity Impact" and "Availability Impact".

## Data health: link the new version to the GitHub release

The "New version x.y.z" pill of the Data health widget opens the About page (release notes). Make it open the release on
GitHub instead, or add a second link: `ReleaseDto.htmlUrl` (already in the API, `getGitHubVersions`) of the newest
release (`latestRelease()` in `HomeDataHealthComponent`). Also a link to the list of the releases of the repository
(`ReleaseOptions.BaseTrivyDashboardRepoUrl`, not in the frontend settings yet) in the Services box, "GitHub releases".

## Home: "open report" button on images that have no digest yet

In the expanded Home widgets, an image shown with its digest has a button (`pi pi-external-link`) that opens its
vulnerability report in a new tab (`HomeImageHistoryService.openReport`, `/open-trivy-rep?page=vulnerability-reports&digest=...`).
Done: New images and History drift (Selected box; History drift also keeps the history button), Oldest images (Selected
box), Base OS (rows and table), the "Show all" tables of New vulnerabilities, New fixes and Known exploited.

Missing, as the server sends only the image names (`ImageNames`, repository:tag), not the digests:

- the Image field of the Selected box (`app-home-selected-nodes`) in New vulnerabilities, New fixes, Known exploited,
  Where is package and Where is vulnerability;
- the Images list box of the Where is shell (vulnerability, package, license);
- Fix waiting: the "Images for selected" list and the list grouped by Image.

To do (backend = DTO change):

- `HomeImageRelations` merges the images of one name with other digests into **one line** (by design). So a name can
  have several digests: send `ImageDigests: IReadOnlyList<IReadOnlyList<string>>` (per image, as `ImageNames`), and on
  the frontend open the report directly for one digest and offer a choice (or open the history) for several. The
  alternative, one line per digest, changes the counts and the graphs (the name would repeat).
- DTOs: `HomeNewVulnerabilityDetailItemDto`, `HomeNewFixDetailItemDto`, `HomeFixWaitingDetailItemDto`,
  `HomeKnownExploitedDetailItemDto` (`HomeDtos.cs`), `HomeWhereIsVulnerabilityDetailItemDto`,
  `HomeWhereIsPackageDetailItemDto`, `HomeWhereIsLicenseDetailItemDto` (`HomeSearchDtos.cs`).
- Regenerate the frontend API (`npm run update-endpoints:local`, `npm run prebuild` in `src/frontend`, backend running).
- Exposed secrets opens the exposed secrets report (by design); a vulnerability report button there is not done either.

## Home filter bar: an open filter panel follows the page when it scrolls

**Where:** the home page filter bar (`home-filter-bar.component.html`): the Changes select, the Severities and Namespaces multiselects and, in the compact layout, the "Filters" popover.

### The bug

Open a filter (for example Severities) and scroll the page: the panel scrolls away with the page, while the bar (the sticky nav) stays on screen. The panel ends up detached from its control, for example 75px below it when the page was scrolled up after it opened.

### Cause

Not a wrong parent: `appendTo="body"` is deliberate (inside the pill the panel would be clipped and stacked under the widgets).

- An overlay in the body is placed with absolute coordinates, computed once when it opens. The bar is stuck to the viewport.
- PrimeNG closes an overlay on scroll only for the scrolling of the ancestors of its control that have `overflow: auto` or `scroll` (`ConnectedOverlayScrollHandler` / `DomHandler.getScrollableParents` in `primeng/dom`, bound by `primeng/overlay`). The document is skipped, and here the whole page is what scrolls, so nothing listens.
- Checked in PrimeNG 21.1.10 by reading its code and from the symptom; not reproduced with a debugger.

### Proposed fix

In `HomeFilterBarComponent`, close the open panel when the page scrolls. Do it cheaply, as the app uses zone.js (`provideZoneChangeDetection({ eventCoalescing: true })` in `app.config.ts`): a plain host listener `(window:scroll)` would run change detection on every scroll event of every page, with or without a panel open.

- Add one **passive** `scroll` listener on the window **only while a panel is open** (the show and hide events of the selects and of the popover attach and detach it).
- Register it **outside the Angular zone** (`NgZone.runOutsideAngular`), so scrolling does not trigger change detection.
- Its handler hides the open panel(s) (`hide()` of the select, multiselect or popover) and removes itself on the first scroll event.
- Scrolling inside a panel's own option list does not fire a window `scroll`, so it does not close it.
- Check the compact layout: the popover contains the selects, so both can be open together.

Cost: nothing while no panel is open (no listener exists); while one is open, one small callback that detaches itself on the first scroll. About 20 lines.

Alternative: leave it. It only shows when scrolling with a filter open and goes away on the next click. Re-positioning the panel on every scroll event instead of closing it is possible, but more fragile.
