using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Infrastructure.Nvd.Models;

namespace TrivyOperator.Dashboard.Infrastructure.Nvd.Serializations;

[JsonSerializable(typeof(NvdCveResponse))]
internal partial class NvdJsonContext : JsonSerializerContext;
