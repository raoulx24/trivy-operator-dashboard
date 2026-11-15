using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Infrastructure.CveOrg.Models;

namespace TrivyOperator.Dashboard.Infrastructure.CveOrg.Serializations;

[JsonSerializable(typeof(CveOrgRecordResponse))]
internal partial class CveOrgJsonContext : JsonSerializerContext;
