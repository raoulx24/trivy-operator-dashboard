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
> The version used in the examples (1.8.0) is not necessarily the latest. Always check for the most recent version on [GitHub Charts](https://github.com/raoulx24/trivy-operator-dashboard/pkgs/container/charts%2Ftrivy-operator-dashboard) before installing.

Steps:

1. Customize `values.yaml` file. The parameters from `# app related params` section are explained in [Specific Parameters](#specific-parameters)
2. if ingress with TLS is needed, update accordingly the values from `ingress` section and create the TLS secret. Example:
```sh
kubectl create secret tls chart-example-tls --cert=path/to/cert/file --key=path/to/key/file
```
3. run the helm. Example:
```sh
helm install trivy-operator-dashboard oci://ghcr.io/raoulx24/charts/trivy-operator-dashboard --version 1.8.0 -f my-custom-values-file.yaml
```

**Optional:** If you want to inspect or unpack the chart locally:
```sh
helm pull oci://ghcr.io/raoulx24/charts/trivy-operator-dashboard --version 1.8.0
tar -xzf trivy-operator-dashboard-1.8.0.tgz
```

## Specific Parameters

In Helm values file, the following sections are app related

### `kubernetes` section

| key name                        | description |
| --------------------------------|-------------|
| kubeConfigFileName              | name of the custom kubeconfig file |
| kubeConfigPath                  | full path for the kubeconfig file. it should not be changed |
| kubeConfigSecretName            | the secret that holds the kubeconfig content | 
| namespaceList                   | comma-separated list of namespaces. Providing this disables the namespaces watcher |
| useDefaultContext               | if `true`, only the default context is used and watchers are enabled. <br>if `false`, all contexts are used, but watchers are disabled
| trivyUse*TrivyReportName*Report | enables or disables the specific *Trivy Report* module - for brevity, the full list is not provided here; in Helm values they are fully provided |

### `history` section
| key name                        | description |
| --------------------------------|-------------|
| enabled              | enables or disables the history feature |

#### `retention` subsection
| key name                        | description |
| --------------------------------|-------------|
| runIntervalInMinutes  | execution interval of the retention job, in minutes |
| keepDays              | number of days to retain vulnerability history. minimum value: 1 |
| keepLast              | minimum number of history entries to retain, regardless of age. minimum value: 2 |

#### `distributedCache` subsection
| key name                        | description |
| --------------------------------|-------------|
| connectionString  | connection string for Redis or Valkey |
| retryOptions      | retry configurations used when connection attempts to Redis or Valkey fail |

#### `sidecar` subsection
| key name                        | description |
| --------------------------------|-------------|
| enabled  | deploys Valkey as a sidecar container |
| image  | container image name and tag |
| pvc  | persistent volume claim (PVC) configuration options |
| securityContext | security context applied to the sidecar container |


### `fileRepository` section

| key name                        | description |
| --------------------------------|-------------|
| enabled                         | enables or disables the file repository feature. in this mode, instead of querying the kubernetes for reports, exported files by the operator are used |
| pvcName                         | the Persistent Volume Claim (PVC) used by Trivy Operator for storing reports as files |
| basePath                        | where the PVC will be mounted and consumed by Trivy Operator Dashboard |
| *TrivyreportName*tCrSubpath     | the subpath for files of the specific *Trivy Report* module - for brevity, the full list is not provided here; in Helm values they are fully provided. If empty, they are ignored. Should not be changed unless you are certain |


### `gitHub` section

| key name                            | description |
| ------------------------------------|-------------|
| serverCheckForUpdates               | enables or disables the backend check for new versions and cache release information. |
| checkForUpdatesIntervalInMinutes    | the time interval in minutes used by the backend for new version polling |


### `openTelemetry` section

| key name                            | description |
| ------------------------------------|-------------|
| enabled                             | enables or disables OpenTelemetry instrumentation |
| otelEndpoint                        | otel endpoint. normally, it is http://otel-endpoint(:port) |
| otelProtocol                        | the protocol used for otel writer. can be `grpc` or `http` |
| consoleEnabled                      | enables or disables console output. Not recommended for production |
| aspNetCoreInstrumentationEnabled    | enables or disables ASPNET instrumentation |
| runtimeInstrumentationEnabled       | enables or disables runtime instrumentation |
| prometheusExporterPort              | port for Prometheus metrics export. **Experimental**; prefer using OpenTelemetry for Prometheus metrics |

> **Note: kube config file**  

1. **Important:** This feature is experimental. If the user provides a kubeconfig, it must be one where all defined connections are actually usable - meaning the authentication plugins work (kubelogin, aws-iam-authenticator, gcloud, OIDC, Vault etc.), the clusters are reachable and with proper RBAC, certificates are valid, and no session or token has expired
2. if a kubeconfig file is provided, the app will ignore the defaults and attempt to use it. If it is malformed, it will fall back to default  
3. `kubeConfigFileName` must be the key used in `kubeConfigSecretName` secret. Failing to do so will block the pod startup  
4. command to create a secret (sample; replace `kubeConfigSecretName`, `kubeConfigFileName`, `path/to/file` with their appropriate values):

   ```sh
   kubectl create secret generic __kubeConfigSecretName__ --from-file=__kubeConfigFileName__=path/to/file
   ```

5. Additional info: [GitHub req](https://github.com/raoulx24/trivy-operator-dashboard/issues/2)

> **Note: default context**  

If set to `true`, only default context will be used and watchers are activated. If set to `false`, all contexts are provided and watchers are disabled (a "passthrough mode" is activated). As a side effect, **all reqs are slower**, as all data from all namespaces is queried in most cases

> **Note: history**

1. **Recommendation:** the sidecar deployment is the recommended installation method. In typical environments, the additional memory consumption is less than 10 MB, even when storing several hundred snapshots
2. **Security:** the bundled Valkey sidecar is configured for pod-local access only and runs with all Linux capabilities dropped
3. Tested versions include Redis 7.2, Valkey 7.2, and Valkey 9
4. Redis is supported but has not been tested as extensively as Valkey. If Redis is already available in the cluster, it can be reused instead of deploying the sidecar
5. TLS connections are supported by the application but have not been validated in all deployment scenarios
6. Redis and Valkey cluster deployments are supported but have not been tested as extensively as standalone deployments. If a cluster deployment already exists in the environment, it can be reused
7. If the sidecar is enabled, a PVC is required to ensure data persistence. A block-storage-backed volume is recommended; network file systems (NFS and similar solutions) have not been validated. Based on observed workloads, approximately 50 IOPS should be sufficient for most installations
8. **Capacity:** history storage usage is lightweight. In observed environments, ~1000 snapshots consume approximately 15MB of memory in Valkey, with RDB persistence being even smaller. Overall growth is bounded by retention settings (keepDays, keepLast) and does not scale linearly without limits. CVE payloads are stored in compressed form (Brotli), which significantly reduces memory and persistence footprint. Other metadata contributes minimally to overall usage
9. Alerts can be defined for detected changes on CVEs. More on [Tips & Tricks - Alerts](./tips-and-tricks.md#alerts-on-vulnerability-reports-history)
10. Redis/Valkey ACL - matrix and command

    | Purpose | Commands | Key Patterns |
    | --------|----------|--------------|
    | Read/write snapshots | HGET, HSET | vr:{\*}:\*:\* |
    | Read/write unprocessed snapshots | HGET, HSET | vr-unprocessed:{\*}:\*:\* |
    | Manage namespace set | SMEMBERS, SADD, SREM | vr:namespaces |
    | TTL | EXPIRE | all above |
    | Scanning | SCAN | all above |

    ```redis
    ACL SETUSER yourappuser on >yourpassword \
      +HGET +HSET \
      +SMEMBERS +SADD +SREM \
      +SCAN +EXPIRE \
      ~vr:{*}:*:* \
      ~vr-unprocessed:{*}:*:* \
      ~vr:namespaces \
      -@admin -@dangerous -@scripting -@slow -@pubsub -@connection
    ```

> **Note: file repository**

1. **Important:** the feature is experimental (as the feature from trivy Operator is not mature)
2. **Important:** if activated, all parameters from `kubernetes`, except `trivyUse*TrivyReportName*Report` ones, are ignored. And no RBAC is needed
3. Minimal additional values that must be set:

   ```yaml
   fileRepository:
     enabled: true
 
   securityContext:
     runAsNonRoot: true
     runAsUser: 10000
   ```

   Affinity for the Trivy Operator pods should also be set, as the PVC is RWO and Trivy Operator Dashboard must run on the same node as Trivy Operator
4. The feature from Trivy Operator is not very mature. Not all Trivy reports are usable. Also, it seems that the files are not cleaned up and grow in size
5. The feature is developed as openly as possible. If new reports become available (and output storage is fixed), it will only require a configuration change during installation (to set the path of that Trivy Report). This is why some parameters have empty values
6. Additinonal info: [GitHub req](https://github.com/raoulx24/trivy-operator-dashboard/issues/7)

> **Note: Open Telemetry and metrics**  

If an OpenTelemetry URL is provided, the Prometheus metrics port should not be used, as OpenTelemetry already supplies the metrics - using both will result in duplication

> **Note: Security Recommendation**  

It is highly recommended that the Prometheus exporter port, if used, be different from the MainAppPort. This separation enhances security by reducing the risk of exposing internal metrics endpoints on public-facing ports. If in doubt, use the recommended ports: 8900 for the app and 8901 for Prometheus

> **Note: Configuration Mapping**  

The parameters described above have corresponding entries in appsettings.json. This file is primarily intended for development purposes and should not be used for production configuration

## Tips & Tricks

Additional documentation (such as how to perform on‑demand scans, how to use the app with an ingress path, and how to set up alerts for new CVEs) can be found in [Tips & Tricks](./tips-and-tricks.md).

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
