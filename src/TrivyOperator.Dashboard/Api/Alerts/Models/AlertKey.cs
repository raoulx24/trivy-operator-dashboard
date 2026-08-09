using TrivyOperator.Dashboard.Application.Alerts.Models;

namespace TrivyOperator.Dashboard.Api.Alerts.Models;

public sealed record AlertKey(string Emitter, EmitterKey EmitterKey);
