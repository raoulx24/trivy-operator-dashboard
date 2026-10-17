using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Infrastructure.EndOfLife.Models;

namespace TrivyOperator.Dashboard.Infrastructure.EndOfLife.Serializations;

[JsonSerializable(typeof(EndOfLifeProductResponse))]
internal partial class EndOfLifeJsonContext : JsonSerializerContext;
