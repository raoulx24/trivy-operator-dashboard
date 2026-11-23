using TrivyOperator.Dashboard.Application.Shared.Jobs;
using TrivyOperator.Dashboard.Application.Shared.Jobs.Abstractions;

namespace TrivyOperator.Dashboard.Composition.Shared;

public static class JobStatusServiceRegistrationExtensions
{
    // the status of the background jobs, pinged by the timed hosted services and read by the home page; it has to be
    // registered before them
    public static void AddJobStatusServices(this IServiceCollection services) =>
        services.AddSingleton<IJobStatusRegistry, JobStatusRegistry>();
}
