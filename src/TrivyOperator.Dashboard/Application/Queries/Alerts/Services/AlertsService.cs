using TrivyOperator.Dashboard.Application.Alerts.Models;
using TrivyOperator.Dashboard.Application.Queries.Alerts.Models;
using TrivyOperator.Dashboard.Application.Queries.Alerts.Services.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache.Abstractions;

namespace TrivyOperator.Dashboard.Application.Queries.Alerts.Services;

public class AlertsService(IConcurrentCache<AlertKey, Alert> cache) : IAlertsService
{
    public Task<IEnumerable<AlertDto>> GetAlertDtos()
    {
        AlertDto[] result = [.. cache.Select(kvp => kvp.Value.ToAlertDto(kvp.Key.Emitter)),];

        return Task.FromResult<IEnumerable<AlertDto>>(result);
    }
}
