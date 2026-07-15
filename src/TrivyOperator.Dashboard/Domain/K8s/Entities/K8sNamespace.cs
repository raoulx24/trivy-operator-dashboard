using TrivyOperator.Dashboard.Domain.K8s.Entities.Abstractions;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

namespace TrivyOperator.Dashboard.Domain.K8s.Entities;

public sealed record K8sNamespace(Uid Id, NamespaceName NamespaceName, ResourceName Name) : IK8sResource;
