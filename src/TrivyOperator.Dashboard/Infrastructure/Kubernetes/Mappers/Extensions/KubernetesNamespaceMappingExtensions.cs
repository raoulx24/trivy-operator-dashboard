using k8s.Models;
using TrivyOperator.Dashboard.Domain.Kubernetes.Entities;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.ToDomain.Extensions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.Mappers.Extensions;

public static class KubernetesNamespaceMappingExtensions
{
    public static KubernetesNamespace ToKubernetesNamespace(this V1Namespace ns, KubernetesNamespace? other)
    {
        Timestamp lastSeen =
            TrivySharedMappingExtensions.ResolveTimestamp(ns.Metadata.CreationTimestamp, DateTime.UtcNow); 
        
        if (other is null || ns.Metadata.Name != other.Name.Value || other.LastSeenAt < lastSeen)
        {
            return new KubernetesNamespace(
                new Uid(ns.Metadata.Uid),
                new NamespaceName(ns.Metadata.Name),
                new ResourceName(ns.Metadata.Name),
                lastSeen
            );
        }

        return other;
    }
}
