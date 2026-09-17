using TrivyOperator.Dashboard.Application.Alerts.Abstractions;
using TrivyOperator.Dashboard.Application.Alerts.Models;
using TrivyOperator.Dashboard.Application.Queries.Alerts.Models;
using TrivyOperator.Dashboard.Application.Queries.Alerts.Services;
using TrivyOperator.Dashboard.Application.Queries.Alerts.Services.Abstractions;
using TrivyOperator.Dashboard.Composition.Shared;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache.Abstractions;

namespace TrivyOperator.Dashboard.Composition.Alerts;

public static class AlertsServiceRegistrationExtensions
{
    public static void AddAlertsRelatedServices(this IServiceCollection services)
    {
        CompositionLogger.Logger?.LogInformation("Adding Alerts related services");
        
        services.AddSignalR();
        services.AddSingleton<IConcurrentCache<AlertKey, Alert>, ConcurrentCache<AlertKey, Alert>>();
        services.AddSingleton<IAlertPublisher, AlertPublisher>();
        services.AddTransient<IAlertsService, AlertsService>();
    }
}
