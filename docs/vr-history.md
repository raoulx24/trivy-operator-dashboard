# Redis/Valkey - Infra Entities

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
<cvesHash> = json with namespaceName, criticalCount, ..., unknownCount, cves
```

```typescript
export interface VrMeta {
  namespaceName: string;
  criticalCount: number;
  highCount: number;
  mediumCount: number;
  lowCount: number;
  unknownCount: number;
  timestamp: string; // first sighting of new cvesHash
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

# Full scan workflow (summary)

When a new scan arrives:
1. Compute cvesHash from sorted CVEs  
2. Check if key `vr:<namespace>:<digest>:<cvesHash>` exists
   - If No -> create string key `vr:<namespace>:<digest>:<cvesHash>` with Brotli JSON
3. Insert/update in `vrmoment:<namespace>:<digest>` `<cvesHash>` with timestamp
4. Insert if not exist in `vrmeta:<namespace>:<digest>` `<cvesHash>` with json

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

### Rebuild

#### Step 1 - Discover snapshots

```redis
SCAN 0 MATCH vr:{<namespace>}:*
```

Group by <digest> is required before retention logic

#### Step 2 - Foreach `<digest>` load metadata

```redis
HGETALL vrmeta:{<namespace>}:<digest>
HGETALL vrmoment:{<namespace>}:<digest>
````

This gives `<cvesHash> = <timestamp>`

##### Step 3 - Repair missing metadata (only when needed)

For each snapshot `<cvesHash>`:
- If `<cvesHash>` does not exist in `vrmeta` or `vrmoment`
  - Decompress snapshot
  - Extract timestamp from JSON
  - Add missing data


### Retention

#### Step 1 - Build working set (start of cleanup part)

Construct in-memory list - `(cvesHash, timestamp)`

Source of timestamp:
- Prefer vrmoment
- Fallback to JSON (only if missing)

#### Step 2 - Apply retention policy

Sort by timestamp (descending). Select:
1. Latest snapshot -> always keep
2. Snapshots within last N days
3. Ensure at least K latest snapshots are kept

Mark remaining snapshots as **candidates for deletion**

#### Step 3 - Delete old snapshots

For each candidate:

```redis
DEL vr:{<namespace>}:<digest>:<cvesHash>
HDEL vrmeta:{<namespace>}:<digest> <cvesHash>
HDEL vrmoment:{<namespace>}:<digest> <cvesHash>
```

### Cleanup (orphanes)

#### Step 1 - Final metadata consistency

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

```csharp
namespace TrivyOperator.Dashboard.Domain.VulnerabilityHistory;

public readonly record struct DigestId(string Value);
public readonly record struct NamespaceName(string Value);
public readonly record struct CvesHash(string Value);
public readonly record struct Timestamp(DateTime Value);

public sealed class VrMeta
{
    public string NamespaceName { get; }
    public long CriticalCount { get; }
    public long HighCount { get; }
    public long MediumCount { get; }
    public long LowCount { get; }
    public long UnknownCount { get; }
    public IReadOnlyList<string> Cves { get; }

    public VrMeta(
        string namespaceName,
        long critical,
        long high,
        long medium,
        long low,
        long unknown,
        IEnumerable<string> cves)
    {
        NamespaceName = namespaceName;
        CriticalCount = critical;
        HighCount = high;
        MediumCount = medium;
        LowCount = low;
        UnknownCount = unknown;
        Cves = cves.ToList().AsReadOnly();
    }
}

public sealed class Snapshot
{
    public DigestId Digest { get; }
    public CvesHash Hash { get; }
    public VulnerabilityReportCr VrCr { get; }

    public Timestamp Timestamp => new(VrCr.Report?.UpdateTimestamp ?? DateTime.MinValue);

    public Snapshot(DigestId digest, CvesHash hash, VulnerabilityReportCr vrCr)
    {
        Digest = digest;
        Hash = hash;
        VrCr = vrCr;
    }
}

public sealed class Digest
{
    public DigestId Id { get; }
    public NamespaceName Namespace { get; }

    private readonly HashSet<CvesHash> _snapshotHashes = new();
    private readonly Dictionary<CvesHash, Timestamp> _lastSeen = new();
    private readonly Dictionary<CvesHash, VrMeta> _metadata = new();

    public IReadOnlyCollection<CvesHash> SnapshotHashes => _snapshotHashes;
    public IReadOnlyDictionary<CvesHash, Timestamp> LastSeen => _lastSeen;
    public IReadOnlyDictionary<CvesHash, VrMeta> Metadata => _metadata;

    public Digest(DigestId id, NamespaceName ns)
    {
        Id = id;
        Namespace = ns;
    }

    public bool HasSnapshot(CvesHash hash) => _snapshotHashes.Contains(hash);

    public void AddSnapshotReference(CvesHash hash, Timestamp timestamp)
    {
        if (_snapshotHashes.Add(hash))
            _lastSeen[hash] = timestamp;
    }

    public void UpdateLastSeen(CvesHash hash, Timestamp timestamp)
    {
        _lastSeen[hash] = timestamp;
    }

    public bool HasMetadata(CvesHash hash) => _metadata.ContainsKey(hash);

    public void AddMetadata(CvesHash hash, VrMeta meta)
    {
        if (!_metadata.ContainsKey(hash))
            _metadata[hash] = meta;
    }

    public IEnumerable<CvesHash> SelectSnapshotsForDeletion(RetentionPolicy policy)
    {
        var ordered = _lastSeen
            .OrderByDescending(x => x.Value.Value)
            .ToList();

        if (ordered.Count == 0)
            return Enumerable.Empty<CvesHash>();

        var keep = new HashSet<CvesHash>();

        // Always keep latest
        keep.Add(ordered[0].Key);

        // Keep within N days
        var cutoff = DateTime.UtcNow.AddDays(-policy.KeepDays);
        foreach (var (hash, ts) in ordered)
        {
            if (ts.Value >= cutoff)
                keep.Add(hash);
        }

        // Keep last K snapshots
        foreach (var (hash, _) in ordered.Take(policy.KeepLast))
            keep.Add(hash);

        return ordered
            .Select(x => x.Key)
            .Where(hash => !keep.Contains(hash));
    }
}

public sealed class RetentionPolicy
{
    public int KeepDays { get; }
    public int KeepLast { get; }

    public RetentionPolicy(int keepDays, int keepLast)
    {
        KeepDays = keepDays;
        KeepLast = keepLast;
    }
}

public interface IDigestRepository
{
    Task<Digest?> Get(NamespaceName ns, DigestId id);
    Task Save(Digest digest);
}

public interface ISnapshotRepository
{
    Task<Snapshot?> Get(DigestId digest, CvesHash hash);
    Task Save(Snapshot snapshot);
    Task Delete(DigestId digest, CvesHash hash);
}

public sealed class ScanIngestionService
{
    private readonly IDigestRepository _digestRepo;
    private readonly ISnapshotRepository _snapshotRepo;

    public ScanIngestionService(
        IDigestRepository digestRepo,
        ISnapshotRepository snapshotRepo)
    {
        _digestRepo = digestRepo;
        _snapshotRepo = snapshotRepo;
    }

    public async Task ProcessScan(NamespaceName ns, VulnerabilityReportCr vrCr)
    {
        var digestId = new DigestId(vrCr.ImageArtifact!.Digest);
        var digest = await _digestRepo.Get(ns, digestId)
                     ?? new Digest(digestId, ns);

        var hash = vrCr.ComputeCvesHash();
        var timestamp = new Timestamp(vrCr.Report!.UpdateTimestamp!.Value);

        if (!digest.HasSnapshot(hash))
        {
            var snapshot = new Snapshot(digestId, hash, vrCr);
            await _snapshotRepo.Save(snapshot);
            digest.AddSnapshotReference(hash, timestamp);
        }
        else
        {
            digest.UpdateLastSeen(hash, timestamp);
        }

        if (!digest.HasMetadata(hash))
        {
            var meta = vrCr.ToVrMeta(ns.Value);
            digest.AddMetadata(hash, meta);
        }

        await _digestRepo.Save(digest);
    }
}

public sealed class RetentionService
{
    private readonly IDigestRepository _digestRepo;
    private readonly ISnapshotRepository _snapshotRepo;

    public RetentionService(
        IDigestRepository digestRepo,
        ISnapshotRepository snapshotRepo)
    {
        _digestRepo = digestRepo;
        _snapshotRepo = snapshotRepo;
    }

    public async Task ApplyRetention(Digest digest, RetentionPolicy policy)
    {
        var toDelete = digest.SelectSnapshotsForDeletion(policy);

        foreach (var hash in toDelete)
        {
            await _snapshotRepo.Delete(digest.Id, hash);
        }

        await _digestRepo.Save(digest);
    }
}

using TrivyOperator.Dashboard.Domain.VulnerabilityHistory;
using System.Security.Cryptography;
using System.Text;

public static class VulnerabilityReportCrExtensions
{
    public static CvesHash ComputeCvesHash(this VulnerabilityReportCr vr)
    {
        var cves = vr.Report?.Vulnerabilities?
            .Select(v => v.VulnerabilityId)
            .OrderBy(x => x)
            .ToArray() ?? Array.Empty<string>();

        var joined = string.Join("|", cves);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(joined));
        return new CvesHash(Convert.ToHexString(bytes));
    }

    public static VrMeta ToVrMeta(this VulnerabilityReportCr vr, string namespaceName)
    {
        var s = vr.Report!.Summary!;
        var cves = vr.Report!.Vulnerabilities?.Select(v => v.VulnerabilityId) 
                   ?? Enumerable.Empty<string>();

        return new VrMeta(
            namespaceName,
            s.CriticalCount,
            s.HighCount,
            s.MediumCount,
            s.LowCount,
            s.UnknownCount,
            cves);
    }
}
```