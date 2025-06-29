Release Notes
===========================

Version 1.0.1 - Apollo (Nov 2024)
------------------------

* Initial release of Trivy Operator Dashboard

Version 1.1 - Boreas (Dec 2024)
------------------------

* Added Cluster Compliance Reports

Version 1.2 Cronus (Dec 2024)
------------------------

* Added Cluster Vulnerability Reports
* Added RBAC Assessment Reports

In work: SBOM Reports

> **Happy Holidays and a Happy New Year!** :-)

Version 1.3 Demeter (Jan 2025)
------------------------

* Major rehaul of Kubernetes Watcher (due to a bug related to runtime)
* C-SBOM and SBOM backends are working

Version 1.3.1 Dike (March 2025)
------------------------
* Watchdog for Kubernetes Watchers

Version 1.4 Erebus (April 2025)
------------------------
* Added SBOM Reports with graph visualization; exports in CycloneDX and SPDX formats
* Direct navigation between Vulnerability Reports and SBOM Reports in both directions
* Instrumentation with OpenTelemetry for metrics and traces
* Major overhaul of About page
* Vulnerability Value Count in pages: values of 0 and null are grayed out (improves visibility)

Version 1.6 Gaia (June 2025)
------------------------
* Introduced the ability to compare two vulnerability reports directly - quickly identify what’s changed and what’s at risk.
* The Inspect/Browse views now feature a draggable splitter between master and detail tables, offering a flexible layout and improved visibility on complex data.
* Upgraded to Angular 19 and PrimeNG 19, paving the way for modern UI features and a more maintainable codebase. Extensive refactoring ensures better performance and future scalability.
* Core backend now runs on the latest .NET 9, accompanied by significant stability refactors and architectural cleanup.
* True support for kubernetes healthz probes (readiness and liveness)
> **Where’s version 1.5?** We’ve jumped a beat - this release includes 120+ commits, which felt a bit much for a mere point upgrade. The bulk of the changes come from the Angular and PrimeNG upgrades, which triggered significant refactoring across the board. 
<br>And in case you're wondering… there are no known Greek (demi)gods whose names start with **F** - so mythologically speaking, version 1.5 simply wasn’t meant to be :-)