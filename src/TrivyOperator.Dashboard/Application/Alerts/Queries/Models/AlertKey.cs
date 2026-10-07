using TrivyOperator.Dashboard.Application.Alerts.Models;

namespace TrivyOperator.Dashboard.Application.Alerts.Queries.Models;

public sealed record AlertKey(string Emitter, EmitterKey EmitterKey);
