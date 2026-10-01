# TO DO

## Frontend

Generic
- replace all functions/methods from htmls with pipes
- chase all styles and replace with classes
- change where aplicable to lazy loading of components in pages (ie vr compare in vr, home pages). maybe deferred load?
- add updatedate, imagedigest
- treat onError - getDataDtos()
- use NavigationExtras.state insted of query params - not possible between tabs

Extend Settings Service (maybe cross tab communication?)

Support /path in ingress - #4

!!! Check Cluster VR missing fields in detailed

there is a ng100 in sboms, on refresh
there is smth wrong with trivyTableSelectedRecords()

## Backend

Proper 200, 404 etc codes in controllers and proper error controll

Github versions - Timed Hosted Service - alert if error

## Both

Export to CycloneDX - server side, zip file, async (signalr?)

## Not clear where and how

Advertise latest version
https://api.github.com/repos/raoulx24/trivy-operator-dashboard/releases/latest

## Misc

Rearrange doc. Maybe wiki?

## Frontend new structure

```txt
app/
│
├── core/
│   ├── interceptors/
│   │   └── kubernetes-context.interceptor.ts
│   │
│   ├── services/
│   │   ├── dark-mode.service.ts
│   │   ├── main-app-init.service.ts
│   │   ├── router-event-emitter.service.ts
│   │   └── title.service.ts
│   │
│   └── state/
│       └── kubernetes-context-state.service.ts
│
├── shared/
│   ├── ui/
│   │   ├── icon/
│   │   ├── mini-bar-chart/
│   │   ├── generic-master-detail/
│   │   └── ...
│   │
│   ├── pipes/
│   │   ├── capitalize-first.pipe.ts
│   │   ├── local-time.pipe.ts
│   │   └── ...
│   │
│   └── utils/
│       ├── color.utils.ts
│       ├── cron.utils.ts
│       ├── number-string.utils.ts
│       └── version.utils.ts
│
├── features/
│   │
│   ├── dashboard/
│   │   ├── dashboard.component.ts
│   │   ├── dashboard.component.html
│   │   ├── dashboard.component.scss
│   │   └── widgets/
│   │       ├── cluster-rbac/
│   │       ├── config-audit/
│   │       ├── exposed-secrets/
│   │       └── vulnerabilities/
│   │
│   ├── alerts/
│   │   ├── alerts.component.ts
│   │   └── ...
│   │
│   ├── settings/
│   │   ├── settings.component.ts
│   │   ├── settings.types.ts
│   │   └── ...
│   │
│   ├── watcher-state/
│   │   ├── watcher-state.component.ts
│   │   ├── watcher-state.constants.ts
│   │   └── ...
│   │
│   ├── history-reports/
│   │   ├── vulnerability-reports-history/
│   │   └── vulnerability-reports-history-detail/
│   │
│   └── trivy-reports/
│       ├── shared/
│       │   ├── abstracts/
│       │   ├── pipes/
│       │   ├── utils/
│       │   └── ui/
│       │
│       ├── vulnerability/
│       │   ├── vulnerability-reports.component.ts
│       │   ├── vulnerability-reports-detail.component.ts
│       │   └── ...
│       │
│       ├── sbom/
│       ├── config-audit/
│       ├── exposed-secrets/
│       ├── rbac-assessment/
│       ├── infra-assessment/
│       └── cluster/
│
├── themes/
│   ├── trivy-operator-dashboard.preset.ts
│   └── trivy-operator-dashboard.variables.css
│
├── app.component.ts
├── app.component.html
├── app.config.ts
└── app.routes.ts

```