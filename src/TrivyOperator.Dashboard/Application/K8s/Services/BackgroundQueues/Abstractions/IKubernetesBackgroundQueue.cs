using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Application.Common.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Application.K8s.Services.WatcherEvents.Abstractions;

namespace TrivyOperator.Dashboard.Application.K8s.Services.BackgroundQueues.Abstractions;

public interface IKubernetesBackgroundQueue<TKubernetesObject> : IBackgroundQueue<IWatcherEvent<TKubernetesObject>>
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>;
