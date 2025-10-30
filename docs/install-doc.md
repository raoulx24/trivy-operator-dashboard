# Installation and Configuration

## Prerequisites

To run, the app needs a Kubernetes cluster. If the app is started without any additional setup ("as is"), needed minimal RBAC rights are as follows

| apiGroup               | resource  | verbs            |
|------------------------|-----------|------------------|
|                        | namespace | get, watch, list |
| aquasecurity.github.io | *         | get, watch, list |

If, for any reason, `watch` on `namespaces` cannot be provided, then the ones for `apiGroup aquasecurity.github.io` are still required, and the value for parameter `namespaceList` must be provided (more info in [Specific Parameters](#specific-parameters)).

> **Note: Why `watch` on `namespaces`**  
> The app starts a watcher on `namespaces` as it needs to be aware of any changes, and to start (or stop) the subsequent watchers on newer (or deleted) namespaces accordingly

## Installation

The recommended way of installation is via helm. The helm package is hosted on [GitHub Container Registry (GHCR)](https://github.com/raoulx24/trivy-operator-dashboard/pkgs/container/charts%2Ftrivy-operator-dashboard), where you can also get the list of versions (tags). Also, the files are provided in `deploy/helm`. The helm is a "standard" one (as obtained by `helm create` command and added specific values and files).

> **Note: Static deploy file**  
> The file `deploy/static/trivy-operator-dashboard.yaml` is a render of the mentioned helm with default values

> **Note: Version in examples**  
> The version used in the examples (1.7.1) is not necessarily the latest. Always check for the most recent version on [GitHub Charts](https://github.com/raoulx24/trivy-operator-dashboard/pkgs/container/charts%2Ftrivy-operator-dashboard) before installing.

Steps:

1. Customize `values.yaml` file. The parameters from `# app related params` section are explained in [Specific Parameters](#specific-parameters)
2. if ingress with TLS is needed, update accordingly the values from `ingress` section and create the TLS secret. Example:
```sh
kubectl create secret tls chart-example-tls --cert=path/to/cert/file --key=path/to/key/file
```
3. run the helm. Example:
```sh
helm install trivy-operator-dashboard oci://ghcr.io/raoulx24/charts/trivy-operator-dashboard --version 1.7.1 -f my-custom-values-file.yaml
```

**Optional:** If you want to inspect or unpack the chart locally:
```sh
helm pull oci://ghcr.io/raoulx24/charts/trivy-operator-dashboard --version 1.7.1
tar -xzf trivy-operator-dashboard-1.7.1.tgz
```

## Specific Parameters

In helm values file, the following parameters are app related:

| section       | key name                            | description |
|---------------| ------------------------------------|-----|
| kubernetes    | kubeConfigFileName                  | path to custom kube config file. normally needed only for dev stage and in k8s it should be empty |
| kubernetes    | namespaceList                       | comma-separated list of namespaces. Providing this disables the namespaces watcher. |
| kubernetes    | trivyUseClusterRbacAssessmentReport | enables or disables Cluster RBAC Assessment Report module; if false, the watchers are disabled |
| kubernetes    | trivyUseConfigAuditReport           | enables or disables Config Audit Report module; if false, the watchers are disabled |
| kubernetes    | trivyUseExposedSecretReport         | enables or disables Exposed Secret Report module; if false, the watchers are disabled |
| kubernetes    | trivyUseVulnerabilityReport         | enables or disables Vulnerability Report module; if false, the watchers are disabled |
| kubernetes    | trivyUseClusterComplianceReport     | enables or disables Cluster Compliance Report module; if false, the watchers are disabled |
| kubernetes    | trivyUseClusterVulnerabilityReport  | enables or disables Cluster Vulnerability Report module; if false, the watchers are disabled |
| kubernetes    | trivyUseRbacAssessmentReport        | enables or disables RBAC Assessment Report module; if false, the watchers are disabled |
| kubernetes    | trivyUseSbomReport                  | enables or disables SBOM Report module; if false, the watchers are disabled |
| kubernetes    | trivyUseClusterSbomReport           | enables or disables Cluster SBOM Report module; if false, the watchers are disabled |
| gitHub        | serverCheckForUpdates               | enables or disables the backend check for new versions and cache release information. |
| gitHub        | checkForUpdatesIntervalInMinutes    | the time interval in minutes used by the backend for new version polling |
| openTelemetry | enabled                             | enables or disables OpenTelemetry instrumentation |
| openTelemetry | otelEndpoint                        | otel endpoint. normally, it is http://otel-endpoint(:port) |
| openTelemetry | otelProtocol                        | the protocol used for otel writer. can be `grpc` or `http` |
| openTelemetry | consoleEnabled                      | enables or disables console output. Not recommended for production |
| openTelemetry | aspNetCoreInstrumentationEnabled    | enables or disables ASPNET instrumentation |
| openTelemetry | runtimeInstrumentationEnabled       | enables or disables runtime instrumentation |
| openTelemetry | prometheusExporterPort              | port for Prometheus metrics export. Experimental; prefer using OpenTelemetry for Prometheus metrics |


> **Note: Configuration Mapping**  
> The parameters described above have corresponding entries in appsettings.json. This file is primarily intended for development purposes and should not be used for production configuration

> **Note: Open Telemetry and metrics**  
> If an OpenTelemetry URL is provided, the Prometheus metrics port should not be used, as OpenTelemetry already supplies the metrics - using both will result in duplication

> **Note: Security Recommendation:**  
> It is highly recommended that the Prometheus exporter port, if used, be different from the MainAppPort. This separation enhances security by reducing the risk of exposing internal metrics endpoints on public-facing ports. If in doubt, use the recommended ports: 8900 for the app and 8901 for Prometheus

## Considerations

### Resources (Requests/Limits)

The app uses caching to deliver fast responses. By storing all data in memory, it significantly reduces the need for repetitive Kubernetes API queries, thereby enhancing performance and minimizing latency, without significant memory overhead. Even though the provided (and commented) resources values are more than enough for some hundreds of scaned images (educated guess is that 500 is a safe number), please do adjust the values based on your needs.

### Running the App

Although there are other means of running the app, such as a "thick client" on a desktop OS, split in frontend and backend, scaled, even in docker (if you insist), they are not in the scope of this document.

### Kubernetes RBAC

In the Kubernetes cluster, there are some other ways of combining RBAC rights. For instance, instead of cluster role, simple namespaced roles can be created. This is a more restricted way of running the app and is pertinent to "multi-tenant clusters" (where same cluster is shared by distinct groups). Also, they are not in the scope of this document.

### Logging - Serilog

The logging component of the backend is based on [Serilog](https://github.com/serilog/serilog/blob/dev/README.md). The file sink can be activated by using `extraEnvValues` from `values.yaml` file, like this:
```yaml
extraEnvValues:
- name: SERILOG__WRITETO__1__NAME
  value: "File"
```
Any other Serilog related parameters can be modified in the same way.

> **Note: Serilog File Sink**  
> Writing directly to container storage without utilizing volumes is strongly discouraged for several critical reasons, including data persistence, security, and resource management. To activate this feature safely and effectively, it is essential to attach a volume to the pod; this is not in the scope of this document

Related to Serilog sinks, only Console and File are present at runtime. If other ones are needed, you can do a custom build of the app or provide them in the image or in the container (via configmap, or init container) and add the needed environment variables in `extraEnvValues` from `values.yaml` file. Also, they are not in the scope of this document.
