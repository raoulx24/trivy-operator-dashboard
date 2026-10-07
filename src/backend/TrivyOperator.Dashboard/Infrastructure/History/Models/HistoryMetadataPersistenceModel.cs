namespace TrivyOperator.Dashboard.Infrastructure.History.Models;

public sealed record HistoryMetadataPersistenceModel(
    string[] NamespaceNames,
    string Registry,
    string Repository,
    string Tag,
    int[] Current,
    string VulnerabilitiesHash,
    int[] AddedCvesDeltas,
    int[] DroppedCvesDeltas
);
