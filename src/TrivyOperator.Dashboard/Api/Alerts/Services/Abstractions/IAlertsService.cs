using TrivyOperator.Dashboard.Api.Alerts.Models;
using TrivyOperator.Dashboard.Application.Alerts.Services;

namespace TrivyOperator.Dashboard.Api.Alerts.Services.Abstractions;

public interface IAlertsService
{
    Task<IEnumerable<AlertDto>> GetAlertDtos();
}
