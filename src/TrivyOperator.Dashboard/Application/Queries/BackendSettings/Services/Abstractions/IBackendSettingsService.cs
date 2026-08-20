using TrivyOperator.Dashboard.Application.Queries.BackendSettings.Models;

namespace TrivyOperator.Dashboard.Application.Queries.BackendSettings.Services.Abstractions;

public interface IBackendSettingsService
{
    Task<BackendSettingsDto> GetBackendSettings();
}
