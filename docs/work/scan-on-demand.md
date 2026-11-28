# Scan on Demand

Scan an image that is not (yet) running in the cluster: give an image reference, pick the report types, and the dashboard asks Trivy Operator to scan it. The reports are kept in Redis/Valkey for a while, and shown in the same tables as the regular reports.

## How it works

1. You submit an image (e.g. `nginx:1.29`) and the report types (vulnerabilities, exposed secrets, SBOM).
2. The dashboard creates a bare Pod in a dedicated namespace. The Pod has an init container that never ends (by default `registry.k8s.io/pause`), so the scanned image is **never pulled by the kubelet and never runs**.
3. Trivy Operator sees the Pod and scans the image, as for any other workload. It pulls the image itself, with the credentials of the Pod (its `imagePullSecrets` and its service account's).
4. As soon as the first selected report appears, the dashboard waits a short grace period for the others, copies them to Redis/Valkey (keyed by digest) and deletes the Pod. Deleting the Pod also deletes Trivy's reports (owner references).
5. The result stays for `ResultTtlHours` (7 days by default), or until you delete it.

The scan namespace is excluded from the main pipeline automatically: the scan Pods and their reports never show up in the regular pages, the history, or the Home widgets.

### Scan states

| State | Meaning |
|---|---|
| Pending | accepted, waiting for a free slot (`MaxConcurrentScans`) |
| Running | the Pod exists, waiting for the reports |
| Completed | the selected reports were collected; a requested type that Trivy did not produce is marked as missing (e.g. exposed secrets disabled in Trivy Operator) |
| Failed | the Pod was refused (missing service account, quota, admission, invalid template, ...) or failed (e.g. the init image could not be pulled) |
| Timed out | no selected report within `ScanTimeoutSeconds`: usually the image does not exist, the registry refused Trivy's pull, or Trivy Operator does not watch the scan namespace |
| Cancelled | cancelled by a user while pending or running; nothing is stored |

> **Note: why "Timed out" and not a precise reason**  
> The image is pulled by Trivy's scan job, in Trivy Operator's namespace, not by the scan Pod. A failed pull leaves nothing in the scan namespace to read, so the dashboard only sees that no report arrives.

## Requirements

- **Redis/Valkey**: the scans and the results are stored there. The connection string is the one of the history (`History:DistributedCache:ConnectionString`).
- **Default context, no file repository**: like the history, scan on demand runs only with `Kubernetes:UseDefaultContext` = true and without the file repository.
- **A dedicated namespace**, created by you (the dashboard never creates namespaces). Do not use a namespace that runs anything else.
- **Trivy Operator must scan that namespace**: if it is configured with `targetNamespaces` or `excludeNamespaces`, include it.
- **RBAC**, in the scan namespace only (see [the samples](#rbac)).
- **A single dashboard replica**: two replicas would process the same scans.

At startup the dashboard checks the namespace, the Pod template and the permissions (with `SelfSubjectAccessReview`). If a check fails, scan on demand is turned off (the rest of the app runs) and the reason is logged and returned by `/api/backend-settings` (`scanOnDemandUnavailableReason`).

## Settings

All in the `ScanOnDemand` section (environment variables: `SCANONDEMAND__<NAME>`, e.g. `SCANONDEMAND__ENABLED`).

| Setting | Default | Description |
|---|---|---|
| `Enabled` | `false` | turns the feature on |
| `NamespaceName` | `trivy-scans` | the dedicated scan namespace |
| `ServiceAccountName` | empty | the service account of the scan Pods (its `imagePullSecrets` are used by Trivy); empty: the namespace's `default` |
| `ImagePullSecretName` | empty | a pull secret put on the scan Pods; must exist in the scan namespace (the dashboard only references it, never reads it) |
| `InitContainerImage` | `registry.k8s.io/pause:3.10` | the blocking init container; mirror it for air-gapped clusters |
| `InitContainerCommand` | empty | its command; empty for pause. For an image like alpine: `tail`, `-f`, `/dev/null` (env: `SCANONDEMAND__INITCONTAINERCOMMAND__0`, `__1`, ...) |
| `PodTemplateFile` | empty | path to a bare Pod YAML used as the base of every scan Pod (see [Pod template](#pod-template)); empty: the built-in spec |
| `AllowedReportTypes` | `VulnerabilityReport,ExposedSecretReport,SbomReport` | the report types a user may pick (comma delimited) |
| `RegistryAllowList` | empty | registries that may be scanned, comma delimited: exact (`myregistry.io`, `localhost:5000`) or a leading wildcard (`*.azurecr.io`, subdomains only). Empty: all. Docker Hub is `docker.io` |
| `RegistryDenyList` | empty | registries that may not be scanned; wins over the allow list |
| `ScanTimeoutSeconds` | `900` | no selected report by then: Timed out |
| `ReportsGracePeriodSeconds` | `30` | after the first selected report, how long to wait for the others |
| `ResultTtlHours` | `168` | how long a result (and its scans) is kept |
| `MaxConcurrentScans` | `3` | scan Pods at the same time |
| `MaxQueuedScans` | `20` | pending + running scans; a submit beyond it is refused |
| `PollIntervalSeconds` | `5` | how often the dashboard checks the running scans (a submit wakes it up earlier) |

## Pull credentials

Trivy pulls the image with the credentials of the scan Pod. Pick one:

- **a service account** with `imagePullSecrets` (recommended): set `ServiceAccountName`. The registry credentials can then change without touching the dashboard;
- **a pull secret** on the Pod: set `ImagePullSecretName`;
- **nothing**, for public images, or when Trivy Operator itself is set up for the registry (e.g. a cloud registry with workload identity on its scan jobs).

> **Note: node identity**  
> On AKS/EKS/GKE the kubelet can pull from the cloud registry with the node's identity, but Trivy's scan job does not use it. If scans of a cloud registry time out, configure Trivy Operator's own registry access, or use a pull secret.

## Pod template

Without a template, the scan Pod is:

- `restartPolicy: Never`, `automountServiceAccountToken: false`, `enableServiceLinks: false`, `terminationGracePeriodSeconds: 0`, `activeDeadlineSeconds` (timeout + grace period + 5 minutes, a safety net if the dashboard is gone);
- Pod security context: `runAsNonRoot: true`, `seccompProfile: RuntimeDefault` (no `runAsUser`: platforms like OpenShift assign their own);
- both containers: no privilege escalation, read-only root filesystem, all capabilities dropped, small requests and limits.

A template replaces that base. It must be a bare `Pod`, with at most one container and one init container. The dashboard always sets:

- `metadata.name`, `metadata.namespace`, and adds its two labels (`app.kubernetes.io/managed-by`, `trivy-operator-dashboard/scan-id`); your other labels and **annotations are kept**;
- `restartPolicy`, `automountServiceAccountToken`, `activeDeadlineSeconds`;
- `serviceAccountName` / `imagePullSecrets`, when the settings set them (otherwise the template's are kept);
- the init container's **name, image and command** (from the settings, never from the template: an init container that ends would let the scanned image run);
- the target container's name and image.

From the containers of the template, only `securityContext` and `resources` are kept (and `env` for the target container). Everything else in `spec` is kept as written: the Pod `securityContext`, `nodeSelector`, `tolerations`, `affinity`, `priorityClassName`, ...

> **Note: a template is used as written**  
> If the template gives a container no `securityContext` or `resources`, the Pod gets none (the platform or a LimitRange fills them). The built-in defaults apply only without a template.

The template is read once, at startup. An invalid template turns scan on demand off, with the reason.

## Samples

### Namespace, service account and pull secret

```yaml
apiVersion: v1
kind: Namespace
metadata:
  name: trivy-scans
---
apiVersion: v1
kind: ServiceAccount
metadata:
  name: trivy-scan
  namespace: trivy-scans
imagePullSecrets:
  - name: my-registry
```

```sh
kubectl create secret docker-registry my-registry -n trivy-scans \
  --docker-server=myregistry.azurecr.io --docker-username=<user> --docker-password=<password>
```

Then `ServiceAccountName: trivy-scan`.

### RBAC

The dashboard's ClusterRole already reads the Trivy reports everywhere (`aquasecurity.github.io`, `get/watch/list`). Scan on demand adds the Pods of the scan namespace:

```yaml
apiVersion: rbac.authorization.k8s.io/v1
kind: Role
metadata:
  name: trivy-operator-dashboard-scans
  namespace: trivy-scans
rules:
  - apiGroups: [""]
    resources: ["pods"]
    verbs: ["create", "get", "list", "delete"]
---
apiVersion: rbac.authorization.k8s.io/v1
kind: RoleBinding
metadata:
  name: trivy-operator-dashboard-scans
  namespace: trivy-scans
roleRef:
  apiGroup: rbac.authorization.k8s.io
  kind: Role
  name: trivy-operator-dashboard-scans
subjects:
  - kind: ServiceAccount
    name: trivy-operator-dashboard    # the dashboard's service account
    namespace: trivy-operator-dashboard
```

No access to Secrets is needed.

### Settings with the current Helm chart

The chart has no dedicated values for scan on demand yet; use `extraEnvValues`:

```yaml
history:
  enabled: true      # the chart sets the Redis/Valkey connection (and the Valkey sidecar) only when history is enabled

extraEnvValues:
  - name: SCANONDEMAND__ENABLED
    value: "true"
  - name: SCANONDEMAND__NAMESPACENAME
    value: "trivy-scans"
  - name: SCANONDEMAND__SERVICEACCOUNTNAME
    value: "trivy-scan"
  - name: SCANONDEMAND__REGISTRYDENYLIST
    value: "docker.io"
  - name: SCANONDEMAND__ALLOWEDREPORTTYPES
    value: "VulnerabilityReport,ExposedSecretReport"
```

> **Note: Redis/Valkey without history**  
> With `history.enabled: false`, the chart sets neither the connection string nor the Valkey sidecar, and the dashboard falls back to `localhost:6379`. Keep history enabled, or provide `HISTORY__DISTRIBUTEDCACHE__CONNECTIONSTRING` yourself.

### Air-gapped init container

```yaml
extraEnvValues:
  - name: SCANONDEMAND__INITCONTAINERIMAGE
    value: "myregistry.local/mirror/pause:3.10"
```

An image without a blocking entrypoint needs a command:

```yaml
extraEnvValues:
  - name: SCANONDEMAND__INITCONTAINERIMAGE
    value: "myregistry.local/mirror/alpine:3.22"
  - name: SCANONDEMAND__INITCONTAINERCOMMAND__0
    value: "tail"
  - name: SCANONDEMAND__INITCONTAINERCOMMAND__1
    value: "-f"
  - name: SCANONDEMAND__INITCONTAINERCOMMAND__2
    value: "/dev/null"
```

### Pod template: OpenShift (restricted SCC)

No user ids: the SCC assigns them.

```yaml
apiVersion: v1
kind: Pod
metadata:
  name: placeholder        # replaced
spec:
  securityContext:
    runAsNonRoot: true
    seccompProfile:
      type: RuntimeDefault
  initContainers:
    - name: placeholder    # name, image and command replaced
      securityContext:
        allowPrivilegeEscalation: false
        capabilities:
          drop: ["ALL"]
      resources:
        requests: { cpu: 1m, memory: 8Mi }
        limits: { cpu: 10m, memory: 16Mi }
  containers:
    - name: placeholder    # name and image replaced
      securityContext:
        allowPrivilegeEscalation: false
        capabilities:
          drop: ["ALL"]
      resources:
        requests: { cpu: 1m, memory: 8Mi }
        limits: { cpu: 10m, memory: 16Mi }
```

### Pod template: service mesh, fixed user, dedicated nodes

```yaml
apiVersion: v1
kind: Pod
metadata:
  name: placeholder
  labels:
    team: secops
  annotations:
    sidecar.istio.io/inject: "false"   # no proxy sidecar (it would be scanned too)
spec:
  priorityClassName: low-priority
  nodeSelector:
    workload: tools
  tolerations:
    - key: dedicated
      operator: Equal
      value: tools
      effect: NoSchedule
  securityContext:
    runAsNonRoot: true
    runAsUser: 65535
    runAsGroup: 65535
    seccompProfile:
      type: RuntimeDefault
  initContainers:
    - name: placeholder
      securityContext:
        allowPrivilegeEscalation: false
        readOnlyRootFilesystem: true
        capabilities:
          drop: ["ALL"]
      resources:
        requests: { cpu: 1m, memory: 8Mi }
        limits: { cpu: 10m, memory: 16Mi }
  containers:
    - name: placeholder
      securityContext:
        allowPrivilegeEscalation: false
        readOnlyRootFilesystem: true
        capabilities:
          drop: ["ALL"]
      resources:
        requests: { cpu: 1m, memory: 8Mi }
        limits: { cpu: 10m, memory: 16Mi }
```

### Mounting the template

Keep the template in a ConfigMap:

```sh
kubectl create configmap scan-pod-template -n trivy-operator-dashboard --from-file=scan-pod.yaml
```

Mount it in the dashboard's Deployment and point `PodTemplateFile` at it:

```yaml
spec:
  template:
    spec:
      containers:
        - name: trivy-operator-dashboard
          env:
            - name: SCANONDEMAND__PODTEMPLATEFILE
              value: /config/scan-pod.yaml
          volumeMounts:
            - name: scan-pod-template
              mountPath: /config
              readOnly: true
      volumes:
        - name: scan-pod-template
          configMap:
            name: scan-pod-template
```

> **Note: Helm**  
> The current chart cannot mount extra volumes; until it does, this needs a patched Deployment (e.g. with kustomize). The template is read at startup, so restart the dashboard after changing it.

## Using it

- **Tools > Scan on Demand** (the entry is greyed out when the feature is off).
- The page shows the submit form and the list of scans. The list updates by itself (SignalR); if the connection is lost, it falls back to polling.
- **Inspect** (a completed scan) folds the list into a narrow rail and shows the result: a header with the image, digest, OS and severity counts, and one tab per report type. The vulnerability and exposed secret tabs are the tables of the regular pages; the SBOM tab is the regular SBOM view.
- **Re-scan** submits the same image and report types again. Submitting an image that is already pending or running returns that scan.
- **Cancel** a pending or running scan: its Pod is deleted and nothing is stored.
- **Delete** a finished scan removes it from the list (its result stays). **Delete** in the result removes the result and all its scans.
- The inspected result is remembered across a reload (no query parameters).

## Limits

- One result per digest: scanning the same image again replaces its result.
- A cancel request is kept in memory until the dashboard acts on it (milliseconds); it is lost if the dashboard restarts in between.
- `requestedBy` is empty until authentication is in place; everyone sees every scan.
- The SBOM of a scan cannot be exported yet (CycloneDX / SPDX).
