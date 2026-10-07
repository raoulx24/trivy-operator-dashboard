using TrivyOperator.Dashboard.Application.BackendSettings.Queries.Models;

namespace TrivyOperator.Dashboard.Application.BackendSettings.Queries.Services.Abstractions;

public interface IBackendSettingsService
{
    Task<BackendSettingsDto> GetBackendSettings();
}
