using TrivyOperator.Dashboard.Api.BackendSettings.Models;

namespace TrivyOperator.Dashboard.Api.BackendSettings.Services.Abstractions;

public interface IBackendSettingsService
{
    Task<BackendSettingsDto> GetBackendSettings();
}
