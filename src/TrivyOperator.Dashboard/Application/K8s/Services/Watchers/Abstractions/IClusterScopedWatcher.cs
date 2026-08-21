using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Application.K8s.Pipeline;

namespace TrivyOperator.Dashboard.Application.K8s.Services.Watchers.Abstractions;

public interface IClusterScopedWatcher : IKubernetesWatcher;
