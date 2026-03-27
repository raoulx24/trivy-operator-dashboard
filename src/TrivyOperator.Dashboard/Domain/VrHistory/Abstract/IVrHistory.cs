using TrivyOperator.Dashboard.Domain.DistributedCache;

namespace TrivyOperator.Dashboard.Domain.VrHistory.Abstract;

public interface IVrHistory
{
    // Snapshot operations
    Task SaveSnapshotAsync(VrSnapshot snapshot);  // vr:{namespace}:{digest}:{cvesHash}
    Task<byte[]> GetSnapshotAsync(string namespaceName, string digest, string cvesHash); // vr:{namespace}:{digest}:{cvesHash}

    // Metadata operations
    Task UpdateMetadataAsync(VrMetadata metadata); // vrmeta:{namespace}:{digest}
    Task<Dictionary<string, VrSummary>> GetMetadataAsync(string @namespace, string digest); // vrmeta:{namespace}:{digest}

    // Image lineage operations
    Task UpdateImageLineageAsync(VrImageLineage imageLineage); // vrimage:{namespace}
    Task<Dictionary<string, string>> GetImageLineageAsync(string @namespace); // vrimage:{namespace}
}
