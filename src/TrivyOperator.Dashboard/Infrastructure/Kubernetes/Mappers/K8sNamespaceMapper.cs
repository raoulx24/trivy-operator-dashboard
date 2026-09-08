using k8s.Models;
using TrivyOperator.Dashboard.Domain.Kubernetes.Entities;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Mappers.Abstract;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Mappers.Extensions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.ToDomain.Extensions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.Mappers;

public class K8sNamespaceMapper :
    IResourceMapper<V1Namespace, K8sNamespace>,
    IResourceKeyProvider<V1Namespace, Uid>
{
    public K8sNamespace MapToDomain(V1Namespace ns, K8sNamespace? existing)
    {
        return ns.ToK8sNamespace(existing);
    }

    public Uid GetKey(V1Namespace kubernetesResource) => kubernetesResource.ToUidKey();
}
