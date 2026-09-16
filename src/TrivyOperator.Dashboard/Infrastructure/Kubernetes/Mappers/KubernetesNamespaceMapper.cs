using k8s.Models;
using TrivyOperator.Dashboard.Domain.Kubernetes.Entities;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Mappers.Abstract;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Mappers.Extensions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.ToDomain.Extensions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.Mappers;

public class KubernetesNamespaceMapper :
    IResourceMapper<V1Namespace, KubernetesNamespace>,
    IResourceKeyProvider<V1Namespace, Uid>
{
    public KubernetesNamespace MapToDomain(V1Namespace ns, KubernetesNamespace? existing)
    {
        return ns.ToKubernetesNamespace(existing);
    }

    public Uid GetKey(V1Namespace kubernetesResource) => kubernetesResource.ToUidKey();
}
