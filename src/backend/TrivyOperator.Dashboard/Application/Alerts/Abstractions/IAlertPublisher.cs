using TrivyOperator.Dashboard.Application.Alerts.Models;

namespace TrivyOperator.Dashboard.Application.Alerts.Abstractions;

public interface IAlertPublisher
{
    Task AddAlert(string emitter, Alert alert, CancellationToken ctx = default);

    Task RemoveAlert(string emitter, Alert alert, CancellationToken ctx = default);
}
