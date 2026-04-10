# Redis/Valkey

## Domain Entities

```csharp
public readonly record struct Digest(string Value);
public readonly record struct NamespaceName(string Value);
public readonly record struct CvesHash(string Value);
public readonly record struct Timestamp(DateTime Value);
```

```csharp
public sealed class VrSnapshot
{
    public NamespaceName NamespaceName { get; } 
    public Digest Digest { get; }
    public CvesHash CvesHash { get; }
    public VulnerabilityReportCr VulnerabilityReport { get; }
    public VrMetadata Metadata { get; }

    public VrSnapshot(VulnerabilityReportCr vrCr)
    {
        NamespaceName = new(vrCr.Metadata.NamespaceProperty());
        Digest = new(vrCrReport?.Artifact?.Digest ?? string.Empty); // or throw
        CvesHash = new(...);
        VulnerabilityReport = vrCr;
        Metadata = ...
    }
}
```

```csharp
public sealed class VrMetadata
{
    public NamespaceName NamespaceName { get; init; } = new(string.Empty);
    public int CriticalCount { get; init; } = 0;
    public int HighCount { get; init; } = 0;
    public int MediumCount { get; init; } = 0;
    public int LowCount { get; init; } = 0;
    public int UnknownCount { get; init; } = 0;
    public string[] Cves { get; init; } = [];
}
```

```csharp
public sealed class VrSnapshotIndexEntry
{
    public NamespaceName NamespaceName { get; }
    public Digest Digest { get; }
    public CvesHash CvesHash { get; }
    public VrMetadata Metadata { get; }

    public Timestamp FirstSeenAt { get; }
    public Timestamp LastSeenAt { get; }

    public VrSnapshotIndexEntry(
        NamespaceName namespaceName,
        Digest digest,
        CvesHash cvesHash,
        VrMetadata metadata,
        Timestamp firstSeenAt,
        Timestamp lastSeenAt)
    {
        NamespaceName = namespaceName;
        Digest = digest;
        CvesHash = cvesHash;
        Metadata = metadata;

        FirstSeenAt = firstSeenAt;
        LastSeenAt = lastSeenAt;
    }
}
```

## Redis storage

Full scan snapshot stores the Snapshot with Brotli‑compressed vrcr JSON.

Key:
```
vr:{<namespace>}:<digest>:<cvesHash>
```

Value is of type `hash`. It has 3 fields:
- `snapshot` with value brotli‑compressed VulnerabilityReportCr dto
- `metadata` with the value VrMeta as json
- `firstSeenAt` with the value string (a DateTimeUTC as string) 
- `lastSeenAt` with the value string (a DateTimeUTC as string)

Created when:
- A new scan produces a new cvesHash (i.e., fingerprint not seen before for this digest and namespace)

Updated when:
- Fields `snapshot`, `metadata`, and `firstSeenAt` - never updated
- Field `lastSeenAt` - always - from VulnerabilityReport.Report?.UpdateTimestamp ?? VulnerabilityReport.Metadata.creationTimestamp

## Full scan workflow (summary)

When a new scan arrives:
1. Create the VrSnapshot

2. Check if key `vr:<namespace>:<digest>:<cvesHash>` exists
   - If Yes -> 
   ```redis
   HSET vr:<namespace>:<digest>:<cvesHash> lastSeenAt <snapshot_timestamp>
   ```
   - If No -> create Brotli compressed VrCr JSON

   ```
   HSET vr:<namespace>:<digest>:<cvesHash> snapshot <brotli_compressed> metadata <vrmetadata_json> firstSeenAt <snapshot_timestamp> lastSeenAt <snapshot_timestamp>
   ```
   where `<snapshot_timestamp>` is VulnerabilityReport.Report?.UpdateTimestamp ?? VulnerabilityReport.Metadata.creationTimestamp

## Redis/Valkey Retention

### 1. Purpose

The job ensures that old snapshots are cleaned according to retention rules

### 2. Retention rules

For each (namespace, digest):
- use LastSeenAt as moment
- if all are older then N days, delete all
- keep all snapshots newer than N days (configurable, default 14)
- delete older then N days, but always keep at least a total of K (configurable, default 2) - newer and older. the delete is from older to newer

### 3. Job workflow (per namespace, digest)

#### Step 1 - Build working set

Construct in-memory list - `(cvesHash, lastSeenAt)` (from VrSnapshotIndexEntry)

Source of timestamp - lastSeen from `vr:{<namespace>}:<digest>:<cvesHash>`

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
```

### 5. Failure handling

- Job is idempotent - can be safely retried
- Partial failures may leave extra snapshots (cleaned next run)

### 6. Execution strategy

- Run periodically (configurable interval)
- Process per `(namespace, digest)` independently
- Can be parallelized safely across digests

## Summary

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