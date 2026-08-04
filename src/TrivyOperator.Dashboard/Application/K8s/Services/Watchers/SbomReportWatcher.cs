using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.K8s.Pipeline;
using TrivyOperator.Dashboard.Application.K8s.Services.Options;
using TrivyOperator.Dashboard.Application.K8s.Services.WatcherEvents;
using TrivyOperator.Dashboard.Domain.TrivyOld.SbomReport;
using TrivyOperator.Dashboard.Infrastructure.Clients.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.K8s.CustomResources;
using TrivyOperator.Dashboard.Infrastructure.K8s.Services.Abstractions;

namespace TrivyOperator.Dashboard.Application.K8s.Services.Watchers;

public class SbomReportWatcher(
    INamespacedResourceService<OldSbomReportCr, CustomResourceList<OldSbomReportCr>>
        namespacedResourceService,
    IKubernetesBackgroundQueue<OldSbomReportCr> backgroundQueue,
    IOptions<WatchersOptions> options,
    IMetricsClient metricsClient,
    ILogger<SbomReportWatcher> logger
) : NamespacedWatcher<CustomResourceList<OldSbomReportCr>, OldSbomReportCr, IKubernetesBackgroundQueue<OldSbomReportCr>,
    WatcherEvent<OldSbomReportCr>>(namespacedResourceService, backgroundQueue, options, metricsClient, logger)
{
    protected override void ProcessReceivedKubernetesObject(OldSbomReportCr kubernetesObject)
    {
        if (kubernetesObject.Report != null)
        {
            kubernetesObject.Report.Components.ComponentsComponents = [];
            kubernetesObject.Report.Components.Dependencies = [];
        }

        base.ProcessReceivedKubernetesObject(kubernetesObject);
    }
}
