namespace TrivyOperator.Dashboard.Domain.DistributedCache.Abstract;

public interface IDistributedCache
{
    // Snapshot operations
    Task SaveSnapshotAsync(VRSnapshot snapshot);  // vr:{namespace}:{digest}:{cvesHash}
    Task<byte[]> GetSnapshotAsync(string namespaceName, string digest, string cvesHash); // vr:{namespace}:{digest}:{cvesHash}

    // Metadata operations
    Task UpdateMetadataAsync(VRMetadata metadata); // vrmeta:{namespace}:{digest}
    Task<Dictionary<string, DateTime>> GetMetadataAsync(string @namespace, string digest); // vrmeta:{namespace}:{digest}

    // Image lineage operations
    Task UpdateImageLineageAsync(VRImageLineage imageLineage); // vrimage:{namespace}
    Task<Dictionary<string, string>> GetImageLineageAsync(string @namespace); // vrimage:{namespace}
}
