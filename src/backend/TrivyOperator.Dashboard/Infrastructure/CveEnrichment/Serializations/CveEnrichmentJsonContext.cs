using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Infrastructure.CveEnrichment.Models;

namespace TrivyOperator.Dashboard.Infrastructure.CveEnrichment.Serializations;

[JsonSerializable(typeof(KevCatalogResponse))]
[JsonSerializable(typeof(EpssResponse))]
[JsonSerializable(typeof(EuvdSearchResponse))]
internal partial class CveEnrichmentJsonContext : JsonSerializerContext;
