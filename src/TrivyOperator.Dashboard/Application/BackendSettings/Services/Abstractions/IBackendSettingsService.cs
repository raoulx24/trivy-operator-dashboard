using TrivyOperator.Dashboard.Application.BackendSettings.Models;

namespace TrivyOperator.Dashboard.Application.BackendSettings.Services.Abstractions;

public interface IBackendSettingsService
{
    Task<BackendSettingsDto> GetBackendSettings();
}
