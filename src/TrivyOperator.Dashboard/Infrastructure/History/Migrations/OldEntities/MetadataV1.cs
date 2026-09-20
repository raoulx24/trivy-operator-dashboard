using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.History.Models;

namespace TrivyOperator.Dashboard.Infrastructure.History.Migrations.OldEntities;

public sealed class MetadataV1
{
    public NamespaceName NamespaceName { get; init; } = new(string.Empty);
    public string ImageRegistry { get; init; } = string.Empty;
    public string ImageName { get; init; } = string.Empty;
    public string ImageTag { get; init; } = string.Empty;
    public int CriticalCount { get; init; }
    public int HighCount { get; init; }
    public int MediumCount { get; init; }
    public int LowCount { get; init; }
    public int UnknownCount { get; init; }
    
    public int[] AddedCvesDeltas { get; init; } = [];
    public int[] DroppedCvesDeltas { get; init; } = [];
}

public static class MetadataV1Mapper
{
    public static HistoryMetadataPersistenceModel ToPersistence(this MetadataV1 source, string vulnerabilitiesHash)
    {
        return new HistoryMetadataPersistenceModel(
            NamespaceNames:
            [
                source.NamespaceName.Value
            ],
            Registry: source.ImageRegistry,
            Repository: source.ImageName,
            Tag: source.ImageTag,
            Current:
            [
                source.CriticalCount,
                source.HighCount,
                source.MediumCount,
                source.LowCount,
                source.UnknownCount,
                0
            ],
            VulnerabilitiesHash: vulnerabilitiesHash,
            AddedCvesDeltas: source.AddedCvesDeltas,
            DroppedCvesDeltas: source.DroppedCvesDeltas
        );
    }
}
