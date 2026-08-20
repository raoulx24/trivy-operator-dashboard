using TrivyOperator.Dashboard.Application.K8s.Models.WatcherEvents;
using TrivyOperator.Dashboard.Infrastructure.Utils;

namespace TrivyOperator.Dashboard.Application.K8s.Pipeline;

public interface IKubernetesEventCoordinator
{
    Task Start(WatcherKey key, CancellationToken ctx = default);
}
