using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Infrastructure.History.Models;

namespace TrivyOperator.Dashboard.Infrastructure.History.Serializations;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(HistoryMetadataPersistenceModel))]


internal partial class HistoryPersistenceJsonContext : JsonSerializerContext;