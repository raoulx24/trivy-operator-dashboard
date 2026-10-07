using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Infrastructure.History.Migrations.OldEntities;

namespace TrivyOperator.Dashboard.Infrastructure.History.Migrations.Serializations;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(MetadataV1))]
[JsonSerializable(typeof(VulnerabilityPersistenceV1))]
[JsonSerializable(typeof(VulnerabilityPersistenceV1[]))]

internal partial class PersistenceMigrationsJsonContext : JsonSerializerContext;