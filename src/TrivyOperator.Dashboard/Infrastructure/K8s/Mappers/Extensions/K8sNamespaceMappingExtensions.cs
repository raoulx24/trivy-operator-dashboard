using k8s.Models;
using TrivyOperator.Dashboard.Domain.K8s.Entities;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Extensions;

namespace TrivyOperator.Dashboard.Infrastructure.K8s.Mappers.Extensions;

public static class K8sNamespaceMappingExtensions
{
    public static K8sNamespace ToK8sNamespace(this V1Namespace ns, K8sNamespace? other)
    {
        Timestamp lastSeen =
            TrivySharedMappingExtensions.ResolveTimestamp(ns.Metadata.CreationTimestamp, DateTime.UtcNow); 
        
        if (other is null || ns.Metadata.Name != other.Name.Value || other.LastSeenAt < lastSeen)
        {
            return new K8sNamespace(
                new Uid(ns.Metadata.Uid),
                new NamespaceName(ns.Metadata.Name),
                new ResourceName(ns.Metadata.Name),
                lastSeen
            );
        }

        return other;
    }
}
