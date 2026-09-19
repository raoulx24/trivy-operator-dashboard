using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Models;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.BackgroundQueues.Abstractions;

public interface IKubernetesBackgroundQueue<TKubernetesObject> : IBackgroundQueue<WatcherEvent<TKubernetesObject>>
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>, new();
