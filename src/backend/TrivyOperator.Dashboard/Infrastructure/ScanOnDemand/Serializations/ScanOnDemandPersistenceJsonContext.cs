using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Infrastructure.ScanOnDemand.Models;

namespace TrivyOperator.Dashboard.Infrastructure.ScanOnDemand.Serializations;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(OnDemandScanPersistenceModel))]
[JsonSerializable(typeof(VulnerabilityReportHeaderPersistenceModel))]
[JsonSerializable(typeof(ExposedSecretReportHeaderPersistenceModel))]
[JsonSerializable(typeof(SbomReportHeaderPersistenceModel))]
internal partial class ScanOnDemandPersistenceJsonContext : JsonSerializerContext;
