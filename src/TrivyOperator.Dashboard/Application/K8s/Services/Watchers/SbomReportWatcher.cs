using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.K8s.Services.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Application.K8s.Services.Options;
using TrivyOperator.Dashboard.Application.K8s.Services.WatcherEvents;
using TrivyOperator.Dashboard.Domain.K8s.Abstractions;
using TrivyOperator.Dashboard.Domain.TrivyOld.CustomResources.Abstractions;
using TrivyOperator.Dashboard.Domain.TrivyOld.SbomReport;
using TrivyOperator.Dashboard.Infrastructure.Clients.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.K8s.Services.Abstractions;

namespace TrivyOperator.Dashboard.Application.K8s.Services.Watchers;

public class SbomReportWatcher(
    INamespacedResourceWatchService<SbomReportCr, CustomResourceList<SbomReportCr>>
        namespacedResourceWatchService,
    IKubernetesBackgroundQueue<SbomReportCr> backgroundQueue,
    IOptions<WatchersOptions> options,
    IMetricsClient metricsClient,
    ILogger<SbomReportWatcher> logger
) : NamespacedWatcher<CustomResourceList<SbomReportCr>, SbomReportCr, IKubernetesBackgroundQueue<SbomReportCr>,
    WatcherEvent<SbomReportCr>>(namespacedResourceWatchService, backgroundQueue, options, metricsClient, logger)
{
    protected override void ProcessReceivedKubernetesObject(SbomReportCr kubernetesObject)
    {
        if (kubernetesObject.Report != null)
        {
            kubernetesObject.Report.Components.ComponentsComponents = [];
            kubernetesObject.Report.Components.Dependencies = [];
        }

        base.ProcessReceivedKubernetesObject(kubernetesObject);
    }
}
