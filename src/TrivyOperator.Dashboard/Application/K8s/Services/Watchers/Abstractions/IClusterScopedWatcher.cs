using k8s;
using k8s.Models;

namespace TrivyOperator.Dashboard.Application.K8s.Services.Watchers.Abstractions;

public interface IClusterScopedWatcher<TKubernetesObject> : IKubernetesWatcher<TKubernetesObject>
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>;
