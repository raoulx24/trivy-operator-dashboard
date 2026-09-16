using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Models.WatcherEvents;
using TrivyOperator.Dashboard.Infrastructure.BackgroundQueues.Abstractions;

namespace TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.BackgroundQueues.Abstractions;

public interface IKubernetesBackgroundQueue<TKubernetesObject> : IBackgroundQueue<WatcherEvent<TKubernetesObject>>
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>, new();
