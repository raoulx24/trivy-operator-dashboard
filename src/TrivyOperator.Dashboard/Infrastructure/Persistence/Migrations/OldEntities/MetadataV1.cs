using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

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
