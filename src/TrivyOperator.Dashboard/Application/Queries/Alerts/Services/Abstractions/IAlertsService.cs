using TrivyOperator.Dashboard.Application.Queries.Alerts.Models;

namespace TrivyOperator.Dashboard.Application.Queries.Alerts.Services.Abstractions;

public interface IAlertsService
{
    Task<IEnumerable<AlertDto>> GetAlertDtos();
}
