using TrivyOperator.Dashboard.Domain.DistributedCache;

namespace TrivyOperator.Dashboard.Domain.VrHistory.Abstractions;

public interface IVrHistoryRepository
{
    // Snapshot operations
    Task SaveSnapshotAsync(VrSnapshot snapshot);  // vr:{namespace}:{digest}:{cvesHash}
    Task<VrSnapshot> GetSnapshotAsync(string namespaceName, string digest, string cvesHash); // vr:{namespace}:{digest}:{cvesHash}

    // Metadata operations
    Task UpdateMetadataAsync(VrMetadata metadata); // vrmeta:{namespace}:{digest}
    Task<Dictionary<string, VrSummary>> GetMetadataAsync(string @namespace, string digest); // vrmeta:{namespace}:{digest}

    // Image lineage operations
    Task UpdateImageLineageAsync(VrImageLineage imageLineage); // vrimage:{namespace}
    Task<Dictionary<string, VrImageInfo>> GetImageLineageAsync(string namespaceName); // vrimage:{namespace}
    Task<Dictionary<string, VrImageInfo>> GetImageLineageAsync(string namespaceName, string registry, string repositoryImage); // vrimage:{namespace}
}
