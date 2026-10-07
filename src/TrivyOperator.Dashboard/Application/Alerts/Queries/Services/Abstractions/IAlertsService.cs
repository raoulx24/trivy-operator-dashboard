using TrivyOperator.Dashboard.Application.Alerts.Queries.Models;

namespace TrivyOperator.Dashboard.Application.Alerts.Queries.Services.Abstractions;

public interface IAlertsService
{
    Task<IEnumerable<AlertDto>> GetAlertDtos();
}
