# TO DO

## Frontend

Generic
- replace all functions/methods from htmls with pipes
- chase all styles and replace with classes
- change where aplicable to lazy loading of components in pages (ie vr compare in vr, home pages). maybe deferred load?
- add updatedate, imagedigest
- treat onError - getDataDtos()
- use NavigationExtras.state insted of query params

Extend Settings Service (maybe cross tab communication?)

Support /path in ingress - #4

!!! Check Cluster VR missing fields in detailed

## Backend

Rearrange BuilderServicesExtensions.cs

Proper 200, 404 etc codes in controllers and proper error controll

Add CreationDate in all CRs and dtos (CreationTimestamp vs UpdateTimestamp)

Github versions - Timed Hosted Service - alert if error

(next) change uid in trivy report image dto to latest one (not generated)

Support for `alternateReportStorage` - #7

## Both

Export to CycloneDX - server side, zip file, async (signalr?)

Multi cluster support (in kubernetes? "fat" client?) - #2

## Not clear where and how

Advertise latest version
https://api.github.com/repos/raoulx24/trivy-operator-dashboard/releases/latest

## Misc

Rearrange doc. Maybe wiki?