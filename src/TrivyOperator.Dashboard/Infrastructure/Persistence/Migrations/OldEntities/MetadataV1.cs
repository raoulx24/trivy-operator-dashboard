using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Persistence.History.Models;

namespace TrivyOperator.Dashboard.Infrastructure.Persistence.Migrations.OldEntities;

public sealed class MetadataV1
{
    public NamespaceName NamespaceName { get; init; } = new(string.Empty);
    public string ImageRegistry { get; init; } = string.Empty;
    public string ImageName { get; init; } = string.Empty;
    public string ImageTag { get; init; } = string.Empty;
    public int CriticalCount { get; init; } = 0;
    public int HighCount { get; init; } = 0;
    public int MediumCount { get; init; } = 0;
    public int LowCount { get; init; } = 0;
    public int UnknownCount { get; init; } = 0;
    
    [JsonInclude]
    public int[] AddedCvesDeltas { get; private set; } = [];
    [JsonInclude]
    public int[] DroppedCvesDeltas { get; private set; } = [];
}

public static class MetadataV1Mapper
{
    public static HistoryMetadataPersistenceModel ToPersistence(
        this MetadataV1 source)
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
            AddedCvesDeltas: source.AddedCvesDeltas,
            DroppedCvesDeltas: source.DroppedCvesDeltas
        );
    }
}
