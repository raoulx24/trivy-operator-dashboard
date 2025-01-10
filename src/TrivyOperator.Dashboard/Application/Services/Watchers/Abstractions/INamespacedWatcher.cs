﻿using k8s;
using k8s.Models;

namespace TrivyOperator.Dashboard.Application.Services.Watchers.Abstractions;

public interface INamespacedWatcher<TKubernetesObject> : IKubernetesWatcher<TKubernetesObject>
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>
{
    public void Delete(IKubernetesObject<V1ObjectMeta>? sourceKubernetesObject);
}
