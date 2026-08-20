using TrivyOperator.Dashboard.Application.Alerts.Models;

namespace TrivyOperator.Dashboard.Application.Queries.Alerts.Models;

public sealed record AlertKey(string Emitter, EmitterKey EmitterKey);
