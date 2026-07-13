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

Rearrange BuilderServicesExtensions.cs

Proper 200, 404 etc codes in controllers and proper error controll

Add CreationDate in all CRs and dtos (CreationTimestamp vs UpdateTimestamp)

Github versions - Timed Hosted Service - alert if error

(next) change uid in trivy report image dto to latest one (not generated)

## Both

Export to CycloneDX - server side, zip file, async (signalr?)

Multi cluster support (in kubernetes? "fat" client?) - #2

## Not clear where and how

Advertise latest version
https://api.github.com/repos/raoulx24/trivy-operator-dashboard/releases/latest

## Misc

Rearrange doc. Maybe wiki?

## others

## Architecture summary (pre-Brotli)

### 1. Split Kubernetes capabilities by responsibility

Keep two separate abstractions because they represent different capabilities:

* `IKubernetesWatcher` (event stream)
* `IKubernetesResourcesReader` (read Kubernetes resources)

Even if both are implemented by the same Infrastructure class today, the interfaces should remain separate because they have different responsibilities and lifecycles.

The watcher is a long-lived event source, while the reader is a query capability.

---

### 2. Watching belongs to the Application layer

The orchestration flow is an Application concern:

```text
IKubernetesEventsCoordinator
    ├── starts IKubernetesWatcher(s)
    └── starts IKubernetesEventsDispatcher

Watcher(s)
    ↓
Background Queue
    ↓
Dispatcher
    ↓
IKubernetesEventConsumer(s)
        ├── Cache updater
        ├── Alerts
        ├── Metrics
        └── ...
```

Infrastructure only provides the primitive ability to watch Kubernetes resources.

The coordination, queueing and fan-out belong to Application.

---

### 3. `ITrivyReportStore` is a domain abstraction

`ITrivyReportStore` expresses the ubiquitous language:

* Get by namespace
* Get by digest
* Get by namespace + digest
* etc.

It knows nothing about:

* Kubernetes
* Cache
* Redis
* Files
* HTTP
* Refresh policies

It simply represents "the place from which reports are obtained."

Because it speaks entirely in domain concepts, it is reasonable to place it in the Domain layer.

---

### 4. Infrastructure provides multiple implementations

Depending on configuration, `ITrivyReportStore` can be implemented by:

* In-memory cache
* Redis cache
* Kubernetes passthrough
* File-backed storage

The Application layer does not care which implementation is configured.

---

### 5. Don't overload `ITrivyReportStore`

The in-memory cache intentionally stores reduced SBOMs.

Therefore, it should **not** secretly fetch Kubernetes when someone requests a full SBOM.

That would make the abstraction lie.

Instead, expose a second capability.

For example:

* `IKubernetesResourcesReader`
* `ILiveTrivyReportReader`

This interface represents "read directly from Kubernetes."

---

### 6. Different capabilities, even if methods overlap

It is perfectly acceptable that both interfaces expose similar methods.

For example:

```text
ITrivyReportStore
    GetByNamespace(...)
    GetByDigest(...)

IKubernetesResourcesReader
    GetByNamespace(...)
    GetByDigest(...)
```

The methods look similar, but their semantics differ.

* `ITrivyReportStore`

  * Read from the configured source of truth for the application.
* `IKubernetesResourcesReader`

  * Bypass everything and retrieve the live Kubernetes resource.

That's not duplication; it's expressing different intentions.

---

### 7. Keep source-selection policy out of Infrastructure

Infrastructure should not decide:

* cache vs Kubernetes
* cache vs files
* fallback logic

Those are application policies.

Infrastructure implementations should simply implement their capability.

---

### Resulting architecture

```text
Domain
------
Reports
History
Digest
Namespaces
ITrivyReportStore

Application
-----------
Use cases
IKubernetesEventsCoordinator
IKubernetesEventsDispatcher
IKubernetesEventConsumer(s)

Infrastructure
--------------
IKubernetesWatcher implementation
IKubernetesResourcesReader implementation
In-memory TrivyReportStore
Redis TrivyReportStore
Kubernetes TrivyReportStore
File-backed TrivyReportStore
```

Overall, the architecture converges on a clear separation:

* **Domain** defines *what* reports are and how they are accessed conceptually (`ITrivyReportStore`).
* **Application** orchestrates event processing and use cases.
* **Infrastructure** implements the technical details of watching Kubernetes and providing report data from different backing stores.


## Brotli / compression discussion

### 1. Keep compression out of the Domain

Compression is purely an Infrastructure optimization.

The domain should never contain members like:

```text
byte[] CompressedDetails
```

or know that Brotli (or any codec) exists.

The domain should continue working with fully hydrated `TrivyReport` objects.

---

### 2. Store a stripped report + compressed payload

Instead of introducing a parallel cache DTO hierarchy, use an Infrastructure-only container.

Conceptually:

```text
CacheEntry<TReport>
{
    TReport Summary;
    byte[] Payload;
}
```

Where:

* `Summary` is the domain report recreated with `with`, but without the heavy collection.
* `Payload` contains the compressed details.

For example:

* VulnerabilityReport → compressed vulnerabilities
* SbomReport → compressed components

---

### 3. Rehydrate transparently

The cache implementation should guarantee:

> Every report returned from the cache is fully hydrated.

Internally:

```text
Store:
    Full Report
        ↓
    Extract heavy collection
        ↓
    Compress
        ↓
    Store Summary + Payload

Read:
    Summary + Payload
        ↓
    Decompress
        ↓
    Recreate report with "with"
        ↓
    Return full domain object
```

The rest of the application remains completely unaware.

---

### 4. Use immutable records

Since reports are immutable value objects, using `with` is ideal.

Instead of mutating the report before storing it, create a new stripped instance.

This avoids problems if the original instance is still referenced elsewhere.

---

### 5. Keep compression separate from caching

The cache should **not** know how to compress.

Likewise, the compression component should know nothing about the cache.

Responsibilities become:

```text
Cache
    Stores entries
    Retrieves entries

Codec / Compressor
    Extracts payload
    Compresses
    Decompresses
    Rehydrates
```

The cache simply delegates to the codec.

That makes it easy to benchmark or replace the compression strategy later.

---

### 6. Don't hardcode "Brotli"

Think in terms of a payload codec instead.

Today the implementation may be:

```text
System.Text.Json
        ↓
Brotli
```

Tomorrow it could become:

* MessagePack
* MemoryPack
* Zstd
* another serializer/compressor

The cache implementation shouldn't care.

---

## Alternatives discussed

### Native .NET

#### System.Text.Json

Pros:

* Built into .NET
* Simple
* No dependencies

Can be combined with:

* Brotli
* GZip

---

#### Brotli

Pros:

* Native
* Excellent compression ratio
* Very good for repetitive JSON (like Trivy reports)

Cons:

* More CPU than GZip (mostly during compression)

---

#### GZip

Pros:

* Native
* Simpler
* Faster compression

Cons:

* Worse compression ratio than Brotli

---

## External alternatives

### MessagePack

Pros:

* Much faster serialization
* Smaller payloads than JSON
* Mature

May not even require additional compression.

---

### MemoryPack

Pros:

* Extremely fast
* Very low allocations
* Designed for .NET

Cons:

* Additional dependency
* Binary format

---

### MessagePack + Brotli

Sometimes worthwhile.

Whether it beats plain MessagePack depends on the data.

Requires benchmarking.

---

### Zstandard (Zstd)

Excellent compression/speed balance.

Not part of the .NET BCL.

Requires a third-party library.

---

## What to benchmark

Rather than guessing, benchmark with **real** Trivy reports:

* JSON + Brotli
* JSON + GZip
* MessagePack
* MessagePack + Brotli
* MemoryPack

Measure:

* serialized size
* compressed size
* serialization time
* deserialization time
* compression time
* decompression time
* allocations

---

## Final architectural direction

The cache remains responsible for **caching**.

The codec remains responsible for **encoding/decoding**.

The domain remains responsible for **domain objects**.

That separation keeps the optimization completely inside Infrastructure while allowing you to swap serialization or compression strategies later without changing either the Domain or the cache implementation.
