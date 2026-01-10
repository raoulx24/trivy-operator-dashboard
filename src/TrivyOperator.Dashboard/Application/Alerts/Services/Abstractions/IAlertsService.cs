using TrivyOperator.Dashboard.Application.Alerts.Models;

namespace TrivyOperator.Dashboard.Application.Alerts.Services.Abstractions;

public interface IAlertsService
{
    Task AddAlert(string emitter, Alert alert, CancellationToken cancellationToken);
    
    Task RemoveAlert(string emitter, Alert alert, CancellationToken cancellationToken);

    Task<IEnumerable<AlertDto>> GetAlertDtos();
}
