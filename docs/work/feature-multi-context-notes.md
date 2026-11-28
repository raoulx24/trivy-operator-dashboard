# Multi-context: state, decisions and open points

Handover notes from the watcher rework discussion (release/2.0.0, 2026-11-24), written to start a fresh discussion about multi-context. Paths are relative to `src/backend/TrivyOperator.Dashboard`. Everything listed under "Done" was built and committed by the user. Only slice 1 was observed on a real cluster (memory, normal operation).

## 1. What was done (context for the next discussion)

1. **Watch error handling** (`Infrastructure/Kubernetes/EventPipeline/Services/WatchSessions/`)
   - Failures are classified into Gone, Throttled, ServerTransient, Transport, Unauthorized, Forbidden, NotFound and Unknown (`Failures/WatchFailureClassifier`).
   - Backoff (`WatchRetryPolicy`): exponential, 1s up to 60s, with jitter. `Retry-After` is the minimum wait. 403 and 404 retry every 5 minutes.
   - **Data is cleared only on 410 and on stop.** Also before re-listing after a list that failed partway through. Every other error keeps the data and the `resourceVersion`, and resumes the watch.
   - `resourceVersion` is taken from every event, so a resumed watch doesn't replay events.
   - Pages that fail on a transient error mid-list are retried in place.
   - Watcher status: Yellow, then Red after 5 failures in a row or 5 minutes failing. 403 and 404 are Red at once. Alerts are raised only on Red.
   - Metrics are recorded in processors only, never in the watcher.
   - Malformed custom resources are skipped (`PipelineEventType.Skipped`). Lists are read typed first; only a page that fails to deserialize is re-read one item at a time (`CustomResources/CustomResourcePageReader`). Objects are deserialized straight from the `JsonElement`, with no string copy.
2. **Watcher identity is separated from resource location.**
   - `Domain/Kubernetes/ValueObjects/WatcherScope` (context + namespace, or cluster-wide) is the watcher's identity: registry key, `WatcherId`, gauge, alerts.
   - `ResourceLocation` is where a resource lives.
   - `KubernetesEvent` carries both, as `Scope` and `Location`.
   - `KubernetesWatcherRegistry` serializes start, stop and recreate per scope (a lock per scope).
   - Sessions run on the application's lifetime token, never on a caller's (for example an HTTP request's).
3. **Watch modes and namespace policy** (default context only)
   - Empty `Kubernetes:NamespaceList` → **cluster-wide**: one watcher per namespaced type. Excluded namespaces are filtered on the server with a field selector (`metadata.namespace!=…`). A missing RBAC permission fails visibly (403, Red), which is by design.
   - `Kubernetes:NamespaceList` set → **per namespace**: one watcher per type and namespace.
   - `Kubernetes:ExcludedNamespaceList` applies in both modes. It's also in the Helm chart and the static manifest.
   - `Application/Kubernetes/Namespaces/NamespacePolicyProvider` is fixed at startup. `Reserve()` is meant for scan-on-demand and must be called before the watchers start.
   - The publisher drops events from namespaces the policy rejects.
   - `NamespacedWatchersStarter` (an `IWatchersStarter`) starts the watchers after all pipelines.
   - The namespace watch pipeline and `NamespacedWatcherLifecycleProcessor` are gone from default context. `api/kubernetes-namespaces`, which the frontend doesn't use, returns the static list or a null service.
   - `IResourceStore.ClearContext` handles a `Flushed` from a cluster-wide scope.
4. **Smaller changes**
   - The watcher is Yellow with reason "Listing" during the initial list (`WatcherStateReason`).
   - Page size per type: `Watchers:ResourceListPageSizes`, set to `SbomReport: 50` in `appsettings.json`. The user measured 320 MB total with 20, against about 500 MB with 100.

## 2. Multi-context today (as read in the code)

- **Modes exclude each other** (`Composition/Kubernetes/KubernetesCompositionResolver`):
  - `DefaultContext` gets watchers, the event pipeline, history, home and watcher state.
  - `MultiContext` gets none of those. History and home are turned off in `HistoryCompositionResolver` / `HomeServiceRegistrationExtensions`, and watcher state in `WatcherStateCompositionResolver`.
- **Context per request:** `HttpHeaderKubernetesContextResolver` reads the `X-Kubernetes-Context` header. `KubernetesClientFactory` builds one client per kubeconfig context. `KubernetesContextService` lists them.
- **Data is loaded on request:** `Infrastructure/Kubernetes/Providers/KubernetesResourceProvider` lists everything for the request's context and caches it in `ExpiringResourceDictionaryCache`. Problems with that:
  - **One sliding expiration per type, shared by all contexts.** A busy context keeps idle contexts' data alive, and on expiry every context is dropped together.
  - **One global refresh lock** for all contexts (a comment in the code says so).
  - **Namespaced types reload by looping the namespaces one by one, unpaged** (`NamespacedResourceService.GetResources()` → `GetResourceList(ns)` without a limit). The cluster-wide paged list built for default context (`GetAllNamespacesResourcePage`) isn't used there. Given the SBOM sizes (about 200 KB on average, up to about 3 MB), unpaged lists are a memory risk.
- **`NamespaceList` (static mode) is shared by every context,** but clusters have different namespaces. The namespace policy isn't used in multi-context.

## 3. Findings that block running the watch pipeline per context

These matter if the pipeline is to run for several contexts:

1. **Stores read the context from the ambient resolver, not from the event.**
   - `InMemoryEntityCache` (`Upsert`, `Delete`, `ClearByNamespace`, `ClearContext`) calls `ContextResolver.TryGetCurrentContext`.
   - In the background pipeline there's no HTTP request. `DefaultKubernetesContextResolver` returns the default context; `HttpHeaderKubernetesContextResolver` returns *unset*, which `KubernetesClientFactory.GetClient` turns into the default.
   - **So events from another context would silently land in the default context's store.** `IResourceStore` methods need an explicit context (from `KubernetesEvent.Location.ContextName`).
2. **Resource services also choose their client through the ambient resolver** (`KubernetesResourceService.GetKubernetesClient()`). A watch session for scope `ctx-B` would use the default client. They need the context as a parameter, for example from `WatcherScope.ContextName`. An AsyncLocal accessor exists (`IKubernetesContextAccessor.PushContext`) but isn't used in the pipeline; an explicit parameter is safer.
3. **`NamespacedWatchersStarter` and `ClusterScopedKubernetesEventPipelineStarter` start watchers for the default context only.** They need a loop over the contexts.
4. **Already ready:** `WatcherScope`/`WatcherId` carry the context, so watcher-state rows, alerts and the events gauge are per context. `INamespacePolicyProvider.Get(ContextName)` is the per-context entry point, though it returns the same policy for every context today. The throttle for initial lists is shared across all types and contexts; it could later get a fair share per context.

## 4. User's direction and constraints

- **Memory is the binding constraint.** About 320–500 MB for one cluster; the user is squeezing memory hard. Watching 10 clusters in-process the same way doesn't scale.
- **Redis as the external store** for the bulk of the data, compressed. It's already used for VR history: about 16 MB for 40 days. Redis is a separate piece of work.
- **Reconciliation runs in-process on a timer** (a timed hosted service). No Kubernetes `CronJob`, no separate app: the dashboard runs as a single instance and should stay one app.
- The design should stay open to multi-context, even if the first steps don't implement it.

## 5. Proposals made (not decided)

- **Periodic reconcile per (context, type):**
  - A paged list (cluster-wide where allowed, with the namespace policy applied).
  - Compare it with the previous snapshot by UID and resourceVersion (a small map per context and type).
  - Turn the differences into `Added`/`Modified`/`Deleted` events, fed into **the same event pipeline** the watchers feed.
  - Result: history and home could work in multi-context, deletions come from the comparison, and processors don't need to know whether events came from a watch or a poll.
  - It could also be mixed: watch the default or most active contexts, reconcile the others.
- **Redis behind `IResourceStore`, as one store implementation.**
  - Caution: home aggregations and list filters currently enumerate whole in-memory dictionaries, which isn't viable against compressed Redis data. They'd need precomputed projections, and maybe a small local copy in memory in front of Redis.
  - New `IResourceStore` members shouldn't assume enumerating everything is cheap.
- **Per-context expiration and per-context refresh locks** in the expiring cache, if request-driven loading stays.

## 6. Open questions for the next discussion

1. Which contexts are watched and which are reconciled, and at what interval? How fresh does each need to be?
2. Should configuration be per context: `NamespaceList`, `ExcludedNamespaceList`, enabled reports?
3. How should a context that's unreachable or forbidden be shown? Watcher state per scope already handles per-context Red.
4. In multi-context, does history become per context? The Redis keys would need the context.
5. What's the first vertical slice? A candidate: explicit context in `IResourceStore` and the resource services (findings 3.1 and 3.2), with no behaviour change in default context.

## 7. Small leftovers noticed along the way (not urgent)

- `Configure<BackgroundQueueOptions>("Queues")` is only called from the namespace composition. It was duplicated into the dynamic default-context branch, but it belongs in a place that runs in every mode.
- In static mode, `api/kubernetes-namespaces` still returns excluded namespaces (`StaticNamespaceService` doesn't use the policy). Nothing calls that endpoint.
- Not verified against a cluster:
  - that the `metadata.namespace!=` field selector works on cluster-wide lists and watches of trivy custom resources;
  - that a configured namespace that doesn't exist returns an empty list, not a 404.
- Optional: in cluster-wide mode, the Helm ClusterRole no longer needs `list`/`watch` on `namespaces` for default context.
- Deferred ideas, saved in project memory:
  - "only live reports" (keep only reports of what currently runs; not defined yet);
  - "images without an SBOM": VR digests minus SBOM digests, computed at query time, and only once the watchers have finished listing.
