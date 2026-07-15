using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Infrastructure.BackgroundQueues.Abstractions;

namespace TrivyOperator.Dashboard.Application.K8s.Pipeline;

public interface IKubernetesBackgroundQueue<TKubernetesObject> : IBackgroundQueue<IWatcherEvent<TKubernetesObject>>
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>;
