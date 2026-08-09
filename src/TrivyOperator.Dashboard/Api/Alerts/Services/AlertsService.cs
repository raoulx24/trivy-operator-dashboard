using TrivyOperator.Dashboard.Api.Alerts.Models;
using TrivyOperator.Dashboard.Api.Alerts.Services.Abstractions;
using TrivyOperator.Dashboard.Application.Alerts.Models;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemoryOld.Abstractions;

namespace TrivyOperator.Dashboard.Api.Alerts.Services;

public class AlertsService(IConcurrentCache<AlertKey, Alert> cache) : IAlertsService
{
    public Task<IEnumerable<AlertDto>> GetAlertDtos()
    {
        AlertDto[] result = [.. cache.Select(kvp => kvp.Value.ToAlertDto(kvp.Key.Emitter)),];

        return Task.FromResult<IEnumerable<AlertDto>>(result);
    }
}
