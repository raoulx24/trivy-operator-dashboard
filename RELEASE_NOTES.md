Release Notes
===========================

Version 1.9 Kleio (June 2026)
------------------------
* **Vulnerability Reports History** - introduces a historical timeline of Vulnerability Reports, highlighting how CVEs evolve across snapshots, including severity shifts, package updates, and appearance/disappearance of issues.
* Compare Trivy Reports - comparison of two Trivy Reports is now a first-class feature. Entries are matched using a compound key for accurate side-by-side diffs, a capability heavily used by Vulnerability Reports History.
* Trivy Reports Dependency - the dependency graph has been fully rebuilt and now integrates with Vulnerability Reports History. Provides an at-a-glance overview of all Trivy Reports associated with an image within a namespace, with direct navigation via the Open button.
* New Prometheus Metric - added a metric, `trivyoperatordashboard_history_cve_changes_count_cves_total`, that increments on each new snapshot and tracks added/removed CVEs by severity and namespace, enabling alerting on meaningful vulnerability changes.

Version 1.8 Iapetus (January 2026)
------------------------
* New Reports - added support for the final two missing Trivy report types, **Cluster Infra Assessment Reports** and **Infra Assessment Reports** (finally! 🙂)
* Configuration Enhancements - kubeconfig - added support for custom kubeconfig files and multi‑context environments
* Configuration Enhancements - File Repository - added support for the `alternateReportStorage` setting from the Trivy Operator Helm chart (referred to here as File Repository)
* Ingress Improvements - the application can now be published behind an ingress with a custom path
* DevOps and Build Updates - introduced a multi‑stage, multi‑platform build process; added a new ARM64 build alongside the existing AMD64 build.
* Technological Upgrades - backend migrated to .NET 10, frontend upgraded to Angular 21 and PrimeNG 21. Performed codebase refactoring to improve compatibility and performance.

### (New) Tips & Tricks

Additional documentation (such as how to perform on‑demand scans or how to use the app with an ingress path) can be found in the [Tips & Tricks](https://github.com/raoulx24/trivy-operator-dashboard/blob/master/docs/tips-and-tricks.md) section.

Version 1.7.1 Hestia (October 2025)
------------------------
* Plethora of squashed bugs
* Helm Package, now easily install via OCI, available on GHCR

Version 1.7 Helios (August 2025)
------------------------
* Cluster SBOM - displays Software Bill of Materials of the entire cluster.
* Trivy Reports Dependency - provides a centralized view of all Trivy Reports related to a container image. Available from Vulnerability, Exposed Secrets, and SBOM reports.
* Alerts Page - alerts are accessible via the Notification Bell and organized in a hierarchical tree format with severity, issuer, and category levels. Includes expandable/collapsible nodes and stacked views for detailed counts and messages.
* Prometheus Integration (Experimental) - adds support for exposing dashboard metrics directly to Prometheus.
* Compare Reports - now available across all report types. Uses compound keys for accurate side-by-side comparisons. Differences in values (e.g., installed versions) are displayed in stacked format.
* Watcher Status - includes item counts and supports forceful recreation of watchers.
* Technological Upgrades - migrated to Angular 20 and PrimeNG 20, with associated codebase refactors for compatibility and performance improvements.

Version 1.6 Gaia (June 2025)
------------------------
* Introduced the ability to compare two vulnerability reports directly - quickly identify what’s changed and what’s at risk.
* The Inspect/Browse views now feature a draggable splitter between master and detail tables, offering a flexible layout and improved visibility on complex data.
* Upgraded to Angular 19 and PrimeNG 19, paving the way for modern UI features and a more maintainable codebase. Extensive refactoring ensures better performance and future scalability.
* Core backend now runs on the latest .NET 9, accompanied by significant stability refactors and architectural cleanup.
* True support for kubernetes healthz probes (readiness and liveness)

> **Where’s version 1.5?** We’ve jumped a beat - this release includes 120+ commits, which felt a bit much for a mere point upgrade. The bulk of the changes come from the Angular and PrimeNG upgrades, which triggered significant refactoring across the board. 
>
> And in case you're wondering… there are no known Greek (demi)gods whose names start with **F** - so mythologically speaking, version 1.5 simply wasn’t meant to be :-)

Version 1.4 Erebus (April 2025)
------------------------
* Added SBOM Reports with graph visualization; exports in CycloneDX and SPDX formats
* Direct navigation between Vulnerability Reports and SBOM Reports in both directions
* Instrumentation with OpenTelemetry for metrics and traces
* Major overhaul of About page
* Vulnerability Value Count in pages: values of 0 and null are grayed out (improves visibility)

Version 1.3.1 Dike (March 2025)
------------------------
* Watchdog for Kubernetes Watchers

Version 1.3 Demeter (Jan 2025)
------------------------

* Major rehaul of Kubernetes Watcher (due to a bug related to runtime)
* C-SBOM and SBOM backends are working

Version 1.2 Cronus (Dec 2024)
------------------------

* Added Cluster Vulnerability Reports
* Added RBAC Assessment Reports

In work: SBOM Reports

> **Happy Holidays and a Happy New Year!** :-)

Version 1.1 - Boreas (Dec 2024)
------------------------

* Added Cluster Compliance Reports

Version 1.0.1 - Apollo (Nov 2024)
------------------------

* Initial release of Trivy Operator Dashboard
