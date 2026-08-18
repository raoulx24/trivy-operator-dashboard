namespace TrivyOperator.Dashboard.Infrastructure.Persistence.History.Models;

public sealed record HistoryMetadataPersistenceModel(
    string[] NamespaceNames,
    string Registry,
    string Repository,
    string Tag,
    int[] Current,
    int[] AddedCvesDeltas,
    int[] DroppedCvesDeltas
);
