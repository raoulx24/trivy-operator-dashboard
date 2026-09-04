using k8s.Models;
using TrivyOperator.Dashboard.Domain.K8s.Entities;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.K8s.Mappers.Abstract;
using TrivyOperator.Dashboard.Infrastructure.K8s.Mappers.Extensions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Extensions;

namespace TrivyOperator.Dashboard.Infrastructure.K8s.Mappers;

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
