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