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

# Redis/Valkey - Scan

## 1. Full scan snapshot
Stores the Brotli‑compressed vulnerability report JSON.

Key:
```
vr:{<namespace>}:<digest>:<cvesHash>
```

Value is of type `string`. It is brotli‑compressed VulnerabilityReportCr dto

Created when:
- A new scan produces a new cvesHash (i.e., fingerprint not seen before for this digest and namespace)

Updated when:
- Never updated - immutable snapshot  
- New scan with same cvesHash does not rewrite this key

## 2. Fingerprint metadata (hash)
Tracks when each cvesHash was last seen for a given digest and namespace.

Key:
```
vrmeta:{<namespace>}:<digest>
```

Value (fields) of type `hash`
```
<cvesHash> = json with NamespaceName, Critical, High, Medium, Low, Unknown, CVEs (business names)
```

```typescript
export interface VrMeta {
  namespaceName: string;
  criticalCount: number;
  highCount: number;
  mediumCount: number;
  lowCount: number;
  unknownCount: number;
  cves: string[];
}
```

Created when:
- First time a digest with cvesHash is scanned in a namespace

Updated when:
- Never updated - immutable value  
- New scan with same cvesHash does not rewrite this field

## 3. Last Seen CVE Hash
Stores the last seen timestamp for e CVE hash

Key:
```
vrmoment:{<namespace>}:<digest>
```

Value (fields) of type `hash`
```
<cvesHash> = timestamp (string)
```

Created when:
- First time a digest with cvesHash is scanned in a namespace

Updated when:
- New scan with same cvesHash update the field


## 4. Image lineage (hash)
Tracks all digests ever seen for a given image in a given namespace. vrimage maintains a mapping between digest and its associated image:tag. It is used for grouping digests by image (ignoring tag) and for display purposes. It may be reconstructed from snapshots if missing.
- During ingestion: reflects first observed `image:tag`
- During repair: reconstructed from the oldest snapshot by timestamp (from vrmoment)

It is not guaranteed to be historically accurate (as tag).

Key:
```
vrimage:{<namespace>}
```

Value (fields):
- type: hash
```
<digest> = json with Repository, Image, Tag, Registry
```

```typescript
export interface RepositoryImage {
  Repository: string;
  Image: string;
  Tags: string;
  Registry: string;
}
```

Created when:
- First time any digest of this image is scanned in this namespace

Updated when:
- Never updated - immutable value  
- New scan with same digest does not rewrite this field


## 4. Full scan workflow (summary)

When a new scan arrives:
1. Compute cvesHash from sorted CVEs  
2. Check if key `vr:<namespace>:<digest>:<cvesHash>` exists
   - If No -> create string key `vr:<namespace>:<digest>:<cvesHash>` with Brotli JSON
3. Insert/update in `vrmoment:<namespace>:<digest>` `<cvesHash>` with timestamp
4. Insert if not exist in `vrmeta:<namespace>:<digest>` `<cvesHash>` with json
5. Insert if not exist image lineage (tag is last seen)
   - `HSET vrimage:<namespace> <digest> json`

# Redis/Valkey Retention & Repair Job

## 1. Purpose

The job ensures:
- Old snapshots are cleaned according to retention rules
- Metadata (vrmeta) is consistent with existing snapshots
- Missing or partial metadata is repaired
- At least one snapshot per (namespace, digest) is always preserved

## 2. Source of truth
- Primary: snapshot keys - `vr:<namespace>:<digest>:<cvesHash>`
- Secondary (rebuildable): metadata - `vrmeta:<namespace>:<digest>`

## 3. Retention rules

For each (namespace, digest):
- Always keep at least one snapshot
- Keep all snapshots newer than N days (configurable)
- Optionally keep the last K snapshots (e.g., last 2), regardless of age
- Delete all others

## 4. Job workflow (per digest)

### Step 1 - Discover snapshots

```redis
SCAN 0 MATCH vr:{<namespace>}:*
```

Group by <digest> is required before retention logic

### Step 2 - Foreach `<digest>` load metadata

```redis
HGETALL vrmeta:{<namespace>}:<digest>
HGETALL vrmoment:{<namespace>}:<digest>
````

This gives `<cvesHash> = <timestamp>`

#### Step 3 - Repair missing metadata (only when needed)

For each snapshot `<cvesHash>`:
- If `<cvesHash>` does not exist in `vrmeta` or `vrmoment`
  - Decompress snapshot
  - Extract timestamp from JSON
  - Add missing data

If `<digest>` does not exist in `vrimage:<ns>` -> add missing data extracted from the snapshot with the oldest timestamp (per digest)

#### Step 4 - Build working set (start of cleanup part)

Construct in-memory list - `(cvesHash, timestamp)`

Source of timestamp:
- Prefer vrmoment
- Fallback to JSON (only if missing)

#### Step 5 - Apply retention policy

Sort by timestamp (descending). Select:
1. Latest snapshot -> always keep
2. Snapshots within last N days
3. Ensure at least K latest snapshots are kept

Mark remaining snapshots as **candidates for deletion**

#### Step 6 - Delete old snapshots

For each candidate:

```redis
DEL vr:{<namespace>}:<digest>:<cvesHash>
HDEL vrmeta:{<namespace>}:<digest> <cvesHash>
HDEL vrmoment:{<namespace>}:<digest> <cvesHash>
```

### Step 7 - Final metadata consistency

After deletions:
- Ensure metadata contains only existing snapshots
- Remove any orphaned hash fields:

  ```redis
  HDEL vrmeta:{<namespace>}:<digest> <cvesHash>
  HDEL vrmoment:{<namespace>}:<digest> <cvesHash>
  ```

### Step 8 - VR Image Cleanup - Image-level Retention

Step A - *Compute digest_last_seen*

From:
```redis
HGETALL vrmoment:{<namespace>}:<digest>
```

```
digest_last_seen = max(timestamp)
```

Step B - *Group by image - ignoring tags*

From:
```redis
HGETALL vrimage:<ns>
```
For each `<digest> -> <registry>:<repo>:<image>:<tag>` extract `image_key = <registry>:<repo>:<image>`. Group `image_key -> [digests...]`

Step C - *apply retention per image*

For each image group `Sort digests by digest_last_seen DESC`. Keep:
1. All digests newer than M days
2. At least L newest digests

Delete the rest.

Step D - *Full cleanup per digest*

For each deleted digest:
```redis
DEL vr:{<namespace>}:<digest>:*
DEL vrmeta:{<namespace>}:<digest>
DEL vrmoment:{<namespace>}:<digest>
HDEL vrimage:{<namespace>} <digest>
```

Step E - *implicit image cleanup*

No extra step needed. Because if all digests of an image are deleted, they disappear from vrimage automatically

## 5. Consistency guarantees

After job completion:
- Every snapshot has a corresponding metadata entry
- No metadata entry exists without a snapshot
-- Retention rules are enforced
-- At least one snapshot per digest exists

## 6. Failure handling

- Job is idempotent - can be safely retried
- Partial failures may leave:
  - extra snapshots (cleaned next run)
  - missing metadata (repaired next run)

## 7. Performance considerations

- Decompression occurs **only for snapshots missing in metadata**
- Normal case: no decompression required
- Worst case (full rebuild): all snapshots decompressed

## 8. Execution strategy

- Run periodically (configurable interval)
- Process per `(namespace, digest)` independently
- Can be parallelized safely across digests

## 9. Summary

- Snapshots are authoritative and immutable
- Metadata is a rebuildable index
- Retention is enforced via background job
- Repair is integrated into retention flow
- Hot path (scan ingestion) remains simple and fast
